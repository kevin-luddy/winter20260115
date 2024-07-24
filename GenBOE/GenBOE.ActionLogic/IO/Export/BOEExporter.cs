// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Wordprocessing;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Used for exporting a BOE to a pre-formatted Work template
	/// </summary>
	[ExcludeFromCodeCoverage]
    public class BOEExporter : WordExporter, IBOEExporter
    {
        #region private attributes
        /// <summary>
        /// The permissions dto loader.
        /// </summary>
        private IPermissionsDTODataLoader permissionsDTOLoader;

        /// <summary>
        /// The ad utilities class.
        /// </summary>
        private IActiveDirectoryUtilities adUtils;

        /// <summary>
        /// The common data mapper.
        /// </summary>
        private ICommonDataMapper commonDataMapper; // data in this one is cached, so making calls is not a huge deal

        /// <summary>
        /// The variable select bo eto sum calculation
        /// </summary>
        private IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation;

        /// <summary>
        /// The default hours format
        /// </summary>
        private string DefaultHoursFormat = "###0";
        
        /// <summary>
        /// The workspace decimal precision
        /// </summary>
        private int WorkspaceDecimalPrecision;
        
        /// <summary>
        /// Gets the export converter.
        /// </summary>
        private BOEExportConverter exportConverter;

        #endregion private attributes

        #region Content Types

        public const string CONTENT_TYPE_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string CONTENT_TYPE_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string CONTENT_TYPE_XLSM = "application/vnd.ms-excel.sheet.macroEnabled.12";
        public const string CONTENT_TYPE_CSV = "text/csv";
        public const string CONTENT_TYPE_ZIP = "application/zip";

        #endregion

        #region Field Name Constants

        public const string FieldName_MOQEquation = "BOE:MOQEquation";
        public const string FieldName_MOQOriginalVarsTable = "BOE:MOQOriginalVarsTable";
        public const string FieldName_TaskElementTitle = "BOE:TaskTitle";
        public const string FieldName_TaskElementDescription = "BOE:TaskDescription";
        public const string FieldName_PerfOrg = "BOE:PerfOper";
        public const string FieldName_PerfOrgId = "BOE:PerfOrgId";
        public const string FieldName_SegmentRegion = "BOE:SegmentRegion";
        public const string FieldName_LaborTypes = "BOE:LaborTypes";
        public const string FieldName_LaborTypeID = "BOE:LaborTypeID";
        public const string FieldName_LaborTypeHours = "BOE:LaborTypeHours";
        public const string FieldName_LaborTypeCost = "BOE:LaborTypeCost";
        public const string FieldName_LaborTypeMatSubIwtaCost = "BOE:LaborTypeMatSubIWTACost";
        public const string FieldName_LaborTypeOtherCost = "BOE:LaborTypeOtherCost";
        public const string FieldName_SpreadCurve = "BOE:SpreadCurve";
        public const string FieldName_TaskTypeTitle = "BOE:TaskTypeTitle";
        public const string FieldName_TaskTypeTravelMode = "BOE:TaskTypeTravelMode";
        public const string FieldName_TaskTypeGroupID = "BOE:TaskTypeGroupID";
        public const string FieldName_TaskTypeDescription = "BOE:TaskTypeDescription";
        public const string FieldName_TaskTypeDeparture = "BOE:TaskTypeDeparture";
        public const string FieldName_TaskTypeDestination = "BOE:TaskTypeDestination";
        public const string FieldName_TaskTypeZone = "BOE:TaskTypeZone";
        public const string FieldName_TaskTypePurpose = "BOE:TaskTypePurpose";
        public const string FieldName_TaskTypeTrips = "BOE:TaskTypeTrips";
        public const string FieldName_TaskTypePeople = "BOE:TaskTypePeople";
        public const string FieldName_TaskTypeDays = "BOE:TaskTypeDays";
        public const string FieldName_TaskTypeCars = "BOE:TaskTypeCars";
        public const string FieldName_TaskTypeDate = "BOE:TaskTypeDate";
        public const string FieldName_TaskTypeSkillLevel = "BOE:TaskTypeSkillLevel";
        public const string FieldName_TaskSegregation = "BOE:TaskSegregation";
        public const string FieldName_BOESegregation = "BOE:BOESegregation";
        public const string FieldName_TaskElementRevCode = "BOE:SummaryRevCode";
        public const string FieldName_TaskHoursTotal = "BOE:TaskHoursTotal";
        public const string FieldName_TaskCostTotal = "BOE:TaskCostTotal";
        public const string FieldName_TaskMatSubIWTACostTotal = "BOE:TaskMatSubIWTACostTotal";
        public const string FieldName_TaskOtherCostTotal = "BOE:TaskOtherCostTotal";
        public const string FieldName_TaskTypeSite = "BOE:Site";
        public const string FieldName_TaskTypeSkillMix = "BOE:SkillMix";
        public const string FieldName_ResourceID = "BOE:ResourceID";
        public const string FieldName_GenBOEResourceID = "BOE:GenBOEResourceID";
        public const string FieldName_ResourceName = "BOE:ResourceName";
        public const string FieldName_ResourceDescription = "BOE:ResourceDescription";
        public const string FieldName_ResourceElementOfCost = "BOE:ResourceElementOfCost";
        public const string FieldName_ResourceRateType = "BOE:ResourceRateType";
        public const string FieldName_TravelTripID = "BOE:TravelTripID";
        public const string FieldName_ProductLine = "BOE:ProductLine";
        public const string FieldName_TrackingNumber = "BOE:TrackingNumber";
        public const string FieldName_ProposalSubmittalDate = "BOE:ProposalSubmittalDate";
        public const string FieldName_SummaryTotal = "BOE:SummaryTotal";
        public const string FieldName_ContractStartDate = "BOE:ContractStartDate";
        public const string FieldName_ContractEndDate = "BOE:ContractEndDate";
        public const string FieldName_BOEPWS = "BOE:BOEPWS";
        public const string FieldName_LaborTypeCompany = "BOE:LaborTypeCompany";
        public const string FieldName_LaborTypeCompanyId = "BOE:LaborTypeCompanyId";
        public const string FieldName_LaborTypeSTOT = "BOE:LaborTypeSTOT";
        public const string FieldName_LaborTypeSTOTId = "BOE:LaborTypeSTOTId";
        public const string FieldName_LaborTypeRateBOE = "BOE:LaborTypeRateBOE";
        public const string FieldName_LaborTypeRateBOEId = "BOE:LaborTypeRateBOEId";
        public const string FieldName_ExportFormatId = "ExportFormatId";
        public const string FieldName_PremiumCustomField = "BOE:PremiumCF";
        public const string FieldName_CustomField_SowId = "BOE:CF-SOW-ID";
        public const string FieldName_CustomField_SowDesc = "BOE:CF-SOW-Desc";
        public const string FieldName_TotalHours = "BOE:TotalHours";
        public const string FieldName_IPT = "BOE:IPT";
        public const string FieldName_TaskCostCenter = "BOE:TaskCostCenter";
        public const string FieldName_Asset = "BOE:Asset";
        public const string FieldName_PrintDate = "BOE:PrintDate";
        public const string FieldName_BOETitle = "BOE:BOETitle";
        public const string FieldName_GenBOEBOEID = "BOE:GenBOEBOEID";
        public const string FieldName_ResourceSummaryByResourceIDTable = "BOE:ResourceSummaryByResourceIDTable";
        public const string FieldName_ResourceSummaryByResourceIDLaborCategoryLocationTable = "BOE:ResourceSummaryByResourceIDLaborCategoryLocationTable";
        public const string FieldName_ResourceSummaryContainer = "BOE:ResourceSummaryContainer";
        public const string FieldName_TaskID = "BOE:TaskID";
        public const string FieldName_GenBOETaskID = "BOE:GenBOETaskID";
        public const string FieldName_TaskIDNumber = "BOE:TaskIDNumber";
        public const string FieldName_HoursSummaryRollupContainer = "BOE:HoursSummaryRollupContainer";
        public const string FieldName_HoursSummaryRollup = "BOE:HoursSummaryRollup";
        public const string FieldName_HoursSummaryRollupWithResDesc = "BOE:HoursSummaryRollupWithResDesc";
        public const string FieldName_CostSummaryRollup = "BOE:CostSummaryRollup";
        public const string FieldName_LMSI_NISSCCostSummaryRollup = "BOE:LMSI-NISSCCostSummaryRollup";
        public const string FieldName_CostSummaryRollupWithResDesc = "BOE:CostSummaryRollupWithResDesc";
        public const string FieldName_CostSummaryRollupContainer = "BOE:CostSummaryRollupContainer";
        public const string FieldName_LaborHoursRollup = "BOE:LaborHoursRollup";
        public const string FieldName_GSMOLaborHoursRollup = "BOE:GSMOLaborHoursRollup";
        public const string FieldName_GSMOLaborHoursRollupNoResTotals = "BOE:GSMOLaborHoursRollup-NoResourceTotals";
        public const string FieldName_SMORSLaborHoursRollup = "BOE:SMORSLaborHoursRollup";
        public const string FieldName_SMORSLaborHoursRollupWithPerfOrg = "BOE:SMORSLaborHoursRollupWithPerfOrg";
        public const string FieldName_MSTLaborHoursRollup = "BOE:MSTLaborHoursRollup";
        public const string FieldName_MSTLaborCostRollup = "BOE:MSTLaborCostRollup";
        public const string FieldName_MSTLaborCostRollupContainer = "BOE:MSTLaborCostRollupContainer";
        public const string FieldName_LaborHoursRollupContainer = "BOE:LaborHoursRollupContainer";
        public const string FieldName_TravelDirectCostRollup = "BOE:TravelDirectCostRollup";
        public const string FieldName_GSMOTravelDirectCostRollup = "BOE:GSMOTravelDirectCostRollup";
        public const string FieldName_MSTTravelCostRollup = "BOE:MSTTravelCostRollup";
        public const string FieldName_MSTTravelCostRollupContainer = "BOE:MSTTravelCostRollupContainer";
        public const string FieldName_ODCDirectCostRollup = "BOE:ODCDirectCostRollup";
        public const string FieldName_GSMOODCDirectCostRollup = "BOE:GSMOODCDirectCostRollup";
        public const string FieldName_MSTODCCostRollup = "BOE:MSTODCCostRollup";
        public const string FieldName_MSTODCCostRollupContainer = "BOE:MSTODCCostRollupContainer";
        public const string FieldName_MaterialDirectCostRollup = "BOE:MaterialDirectCostRollup";
        public const string FieldName_GSMOMaterialDirectCostRollup = "BOE:GSMOMaterialDirectCostRollup";
        public const string FieldName_MSTMaterialCostRollup = "BOE:MSTMaterialCostRollup";
        public const string FieldName_MSTMaterialCostRollupContainer = "BOE:MSTMaterialCostRollupContainer";
        public const string FieldName_MaterialTaskElementContainer = "BOE:MaterialTaskElementContainer";
        public const string FieldName_MaterialDetailTaskTitle = "BOE:MaterialDetailTaskTitle";
        public const string FieldName_SummaryByRevCode = "BOE:SummaryByRevCode";
        public const string FieldName_SummaryByDate = "BOE:SummaryByDate";
        public const string FieldName_RevCodeContainer = "BOE:RevCodeContainer";
        public const string FieldName_TaskElementContainer = "BOE:TaskElementContainer";
        public const string FieldName_LaborDetailTaskTitle = "BOE:LaborDetailTaskTitle";
        public const string FieldName_ODCsContainer = "BOE:ODCsContainer";
        public const string FieldName_ODCContainer = "BOE:ODCContainer";
        public const string FieldName_TaskTypeRow_ODC = "BOE:TaskTypeRow-ODC";
        public const string FieldName_TravelsContainer = "BOE:TravelsContainer";
        public const string FieldName_TravelContainer = "BOE:TravelContainer";
        public const string FieldName_TaskTypeRow_Travel = "BOE:TaskTypeRow-Travel";
        public const string FieldName_TaskTypeRow_ZoneTravel = "BOE:TaskTypeRow-ZoneTravel";
        public const string FieldName_TaskTypeRow_NonzoneTravel = "BOE:TaskTypeRow-NonzoneTravel";
        public const string FieldName_HeaderFooter = "BOE:HeaderFooter";
        public const string FieldName_SummaryElementofCost = "BOE:SummaryElementofCost";
        public const string FieldName_SummaryElementofCostHours = "BOE:SummaryElementofCostHours";
        public const string FieldName_SummaryElementofCostCost = "BOE:SummaryElementofCostCost";
        public const string FieldName_BOESpreadTotalsTitle = "BOE:BOESpreadTotalsTitle";
        public const string FieldName_SummaryHourByDateContainer = "BOE:SummaryHourByDateContainer";
        public const string FieldName_SummaryHourByDate = "BOE:SummaryHourByDate";
        public const string FieldName_GSMOSummaryHourByDate = "BOE:GSMOSummaryHourByDate";
        public const string FieldName_MSTSummaryHourByDate = "BOE:MSTSummaryHourByDate";
        public const string FieldName_MSTSummaryHourByDate_NoShading_TopTotal = "BOE:MSTSummaryHourByDate-NoShading-TopTotal";
        public const string FieldName_MSTSummaryHourByDate_TopTotal = "BOE:MSTSummaryHourByDate-TopTotal";
        public const string FieldName_SummaryHourByResource = "BOE:SummaryHourByResource";
        public const string FieldName_SummaryCostByDate = "BOE:SummaryCostByDate";
        public const string FieldName_MSTSummaryCostByDate = "BOE:MSTSummaryCostByDate";
        public const string FieldName_MSTSummaryCostByDate_NoShading_TopTotal = "BOE:MSTSummaryCostByDate-NoShading-TopTotal";
        public const string FieldName_MSTSummaryCostByDate_TopTotal = "BOE:MSTSummaryCostByDate-TopTotal";
        public const string FieldName_SummaryCostByDateContainer = "BOE:SummaryCostByDateContainer";
        public const string FieldName_ProgramName = "BOE:ProgramName";
        public const string FieldName_SolicitationNumber = "BOE:SolicitationNumber";
        public const string FieldName_WBSTitle = "BOE:WBSTitle";
        public const string FieldName_StartDate = "BOE:StartDate";
        public const string FieldName_EndDate = "BOE:EndDate";
        public const string FieldName_BOEDescription = "BOE:BOEDescription";
        public const string FieldName_WBSNumber = "BOE:WBSNumber";
        public const string FieldName_CLIN = "BOE:CLIN";
        public const string FieldName_CLINTitle = "BOE:CLINTitle";
        public const string FieldName_CLINStartDate = "BOE:CLINStartDate";
        public const string FieldName_CLINEndDate = "BOE:CLINEndDate";
        public const string FieldName_Date = "BOE:Date";
        public const string FieldName_SummaryLaborTypes = "BOE:SummaryLaborTypes";
        public const string FieldName_SummaryLaborTypeHours = "BOE:SummaryLaborTypeHours";
        public const string FieldName_SummaryLaborTypeCost = "BOE:SummaryLaborTypeCost";
        public const string FieldName_SummaryCostTotal = "BOE:SummaryCostTotal";
        public const string FieldName_LaborTypesTotal = "BOE:LaborTypesTotal";
        public const string FieldName_LaborCostTotal = "BOE:LaborCostTotal";
        public const string FieldName_SourcesOfData = "BOE:SourcesOfData";
        public const string FieldName_PreparedBy = "BOE:PreparedBy";
        public const string FieldName_PreparedByDate = "BOE:PreparedByDate";
        public const string FieldName_ApprovedBy = "BOE:ApprovedBy";
        public const string FieldName_ApprovedByDate = "BOE:ApprovedByDate";
        public const string FieldName_POP = "BOE:POP";
        public const string FieldName_SOURCEOFDATA = "BOE:SOURCEOFDATA";
        public const string FieldName_SummaryYearTotal = "BOE:SummaryYearTotal";
        public const string FieldName_SummaryYearGrandTotal = "BOE:SummaryYearGrandTotal";
        public const string FieldName_TaskDetailRevCode = "BOE:TaskDetailRevCode";
        public const string FieldName_TaskDetailRevCodeDesc = "BOE:TaskDetailRevCodeDesc";
        public const string FieldName_TaskStartDate = "BOE:TaskStartDate";
        public const string FieldName_TaskEndDate = "BOE:TaskEndDate";
        public const string FieldName_UID = "BOE:UID";
        public const string FieldName_MOQEquationResult = "BOE:MOQEquationResult";
        public const string FieldName_MethodOfQuoting = "BOE:MethodOfQuoting";
        public const string FieldName_MethodOfQuotingLabel = "BOE:MethodOfQuotingLabel";
        public const string FieldName_BOETable = "BOE:BOETable";
        public const string FieldName_Summary = "BOE:Summary";
        public const string FieldName_ResourceContainer = "BOE:ResourceContainer";
        public const string FieldName_ResourceHoursRollup = "BOE:ResourceHoursRollup";
        public const string FieldName_ResourceHoursRollupContainer = "BOE:ResourceHoursRollupContainer";
        public const string FieldName_ResourceCostRollup = "BOE:ResourceCostRollup";
        public const string FieldName_ResourceCostRollupContainer = "BOE:ResourceCostRollupContainer";
        public const string FieldName_ResourceStartDate = "BOE:ResourceStartDate";
        public const string FieldName_ResourceEndDate = "BOE:ResourceEndDate";
        public const string FieldName_BOECustomFields = "BOE:BOECustomFields";
        public const string FieldName_TaskCustomFields = "BOE:TaskCustomFields";
        public const string FieldName_ResCustomFields = "BOE:ResourceCustomFields";
        public const string FieldName_CustomFieldLabel = "BOE:CustomFieldLabel";
        public const string FieldName_CustomFieldID = "BOE:CustomFieldID";
        public const string FieldName_CustomFieldDesc = "BOE:CustomFieldDesc";
        public const string FieldName_ZoneTravelSummary = "BOE:ZoneTravelSummary";
        public const string FieldName_NonzoneTravelSummary = "BOE:NonzoneTravelSummary";
        public const string FieldName_ZoneTravelSummaryTable = "BOE:ZoneTravelSummaryTable";
        public const string FieldName_NonzoneTravelSummaryTable = "BOE:NonzoneTravelSummaryTable";
        public const string FieldName_MultiLabel = "BOE:MultiLabel";
        public const string FieldName_SecondaryResourceName = "BOE:SecondaryResourceName";
        public const string FieldName_SecondaryResourceDescription = "BOE:SecondaryResourceDescription";
        public const string FieldName_Category = "BOE:Category";
        public const string FieldName_ProjectMapResourceSummaryTable = "BOE:ProjectMapResourceSummaryTable";
        public const string FieldName_TieredPercent = "BOE:TieredPercent";
        public const string FieldName_ActivityId = "BOE:ActivityId";
        public const string FieldName_AddDelete = "BOE:AddDelete";
        public const string FieldName_SOWNumber = "BOE:SOWNumber";
        public const string FieldName_SOWTitle = "BOE:SOWTitle";
        public const string FieldName_Rationale = "BOE:Rationale";
        public const string FieldName_ClassOfCost = "BOE:ClassOfCost";
        public const string FieldName_SummaryTableOfHours = "BOE:SummaryTableOfHours";
        public const string FieldName_GovtLaborCategory = "BOE:GovtLaborCategory";
        public const string FieldName_KeyPersonnel = "BOE:KeyPersonnel";
        public const string FieldName_YearLabel = "BOE:YearLabel";
        public const string FieldName_YearValue = "BOE:YearValue";
        public const string FieldName_YearValueTotal = "BOE:YearValueTotal";
        public const string FieldName_BOEDescriptionSSDS = "BOE:BOEDescriptionSSDS";
        public const string FieldName_TaskDescriptionSSDS = "BOE:TaskDescriptionSSDS";
        public const string FieldName_MOQSSDS = "BOE:MethodOfQuotingSSDS";
        private const string SOW_CUSTOM_FIELD = "SOW";
        public const string FieldName_Company = "BOE:Company";
        public const string FieldName_EstimateMethod = "BOE:EstimateMethod";
        public const string FieldName_BoeNumber = "BOE:BOENumber";
        public const string FieldName_SOW = "BOE:SOW";
        public const string FieldName_BoeSummaryTable = "BOE:SummaryTable";
        public const string FieldName_CalendarYear = "BOE:CalendarYear";
        public const string FieldName_SOWResourceTable = "BOE:SOWResourceTable";
        public const string FieldName_LaborCategory = "BOE:LaborCategory";
        public const string FieldName_WSDescription = "WS:Description";
        private const string COMPANY_CUSTOM_FIELD = "Company";
        private const string LOCATION_CUSTOM_FIELD = "Location";
        private const string LABOR_CATEGORY_CUSTOM_FIELD = "LaborCategory";
        private const string RIGHTS_IN_DATA_CUSTOM_FIELD = "Rights in Data";
        public const string FieldName_CustomField_RightsInData = "BOE:RightsInData";
		public const string FieldName_BrcID = "BOE:BrcID";

		#endregion

		/// <summary>
		/// Gets the logger.
		/// </summary>
		protected Logger Logger { get; private set; }

        /// <summary>
        /// Gets the currency formatter.
        /// </summary>
        protected NumberFormatInfo CurrencyFormatter { get; private set; }

        #region Public Functions

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEExporter"/> class.
        /// </summary>
        /// <param name="permissionsDTOLoader">The permissions dto loader.</param>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="variableSelectBOEtoSumCalculation">The variable select bo eto sum calculation.</param>
        /// <param name="ADUtils">The ad utils.</param>
        /// <param name="exportConverter">The export converter.</param>
        public BOEExporter(
            IPermissionsDTODataLoader permissionsDTOLoader,
            IUserDTODataLoader userDTODataLoader,
            ICommonDataMapper commonDataMapper,
            IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation,
            IActiveDirectoryUtilities ADUtils,
            BOEExportConverter exportConverter)
            : base(userDTODataLoader)
        {
            this.permissionsDTOLoader = permissionsDTOLoader;
            this.commonDataMapper = commonDataMapper;
            this.variableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
            this.CurrencyFormatter = new NumberFormatInfo();
            this.CurrencyFormatter.CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR;
            this.CurrencyFormatter.CurrencySymbol = "$";
            this.adUtils = ADUtils;
            this.Logger = new Logger(typeof(BOEExporter));
            this.exportConverter = exportConverter;
        }

        /// <summary>
        /// Does the final document cleanup.
        /// </summary>
        /// <param name="document">The document.</param>
        private void DoFinalDocumentCleanup(WordprocessingDocument document)
        {
            #region Final document cleanup

            #region Set "View" to "Print Layout"

            if (document.MainDocumentPart.DocumentSettingsPart == null)
            {
                document.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
            }

            if (document.MainDocumentPart.DocumentSettingsPart.Settings == null)
            {
                document.MainDocumentPart.DocumentSettingsPart.Settings = new Settings();
            }

            if (document.MainDocumentPart.DocumentSettingsPart.Settings.View == null)
            {
                document.MainDocumentPart.DocumentSettingsPart.Settings.View = new View();
            }

            document.MainDocumentPart.DocumentSettingsPart.Settings.View.Val = ViewValues.Print;

            #endregion

            // TODO - remove header and footer content controls?

            // remove content controls
            WordUtilities.RemoveContentControls(document);
        
            WordUtilities.CleanupDocumentXml(document);

            #endregion
        }

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        /// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
        /// <param name="templatePath">Physical path of the template to copy and populate.</param>
        /// <param name="templateType">Template type</param>
        /// <exception cref="System.ArgumentNullException">Response</exception>
        public void ExportBOEToWordFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace ws, HttpResponseBase response, string fileNameToDisplayToBrowser, string templatePath, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            // setup the response correctly with BufferOutput since this is going to be awhile...
            response.ContentType = CONTENT_TYPE_DOCX;
            response.Clear();
            response.BufferOutput = true;
            response.AppendHeader("Content-Disposition", $"attachment;filename={fileNameToDisplayToBrowser}");

            this.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, templatePath, response.OutputStream, templateType);
        }

        /// <summary>
        /// Export the BOEs as individual files and zip into single download.
        /// </summary>
        /// <param name="exportInputs">The export inputs</param>
        /// <param name="boeExportModelViews">Collection of BOE View Models</param>
        /// <param name="boeSummaryGridModelViews"></param>
        /// <param name="ws">Full workspace</param>
        /// <param name="response">What will ultimately be the response to the requester</param>
        /// <param name="fileNameToDisplayToBrowser">File name that will be passed to browser (for download)</param>
        /// <param name="templatePath">Path to the export template</param>
        /// <param name="templateType">Type of the export template</param>
        public void ExportBOEsToZipFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace workSpace, HttpResponseBase response, string returnFilename, string templatePath, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
        {
            AllBOEExportHelper.ExportBOEsToZipFile<bool>(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workSpace, response, returnFilename, templatePath, ExportBOEToWordFileStream);
        }

        /// <summary>
        /// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
        /// </summary>
        /// <param name="workspace">Workspace containing BOE(s) in export</param>
        public void SetWorkspacePrecisionVariables(WorkspaceDTO workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            this.WorkspaceDecimalPrecision = workspace.DecimalPrecision;
            this.DefaultHoursFormat = Utilities.PrecisionFormattingString(this.WorkspaceDecimalPrecision);
            this.exportConverter.SetWorkspacePrecisionVariables(workspace);
        }

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full Workspace</param>
        /// <param name="templatePath">Physical path of the template to copy and populate.</param>
        /// <param name="returnStream">Output stream</param>
        /// <param name="templateType">Template type</param>
        /// <exception cref="System.ArgumentNullException">if exportInputs is null</exception>
        /// <exception cref="GeneralAppException"></exception>
        /// <returns>true if successful, exception otherwise</returns>
        public bool ExportBOEToWordFileStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace ws, string templatePath, Stream returnStream, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            // If the ModelViews have data
            if (boeExportModelViews != null && boeSummaryGridModelViews != null)
            {

                // template file is on disk
                this.Export(templatePath, (document) =>
                {
                    this.PopulateDataExportBOE(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, templateType, document);
                }, returnStream);
            }

            return true;
        }

        /// <summary>
        /// Populates the data export BOE.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="templateType">Template type</param>
        /// <param name="document">The openxml word document.</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateDataExportBOE(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, FullWorkspace ws,
            ExcelReportTemplateType templateType, WordprocessingDocument document)
        {
            ChunkCounter counters = new ChunkCounter();

            bool writelogstatements = ConfigurationUtilities.GetAppSetting<bool>("ExportLoggingEnableInternalMV", false);

            if (FullObjectHelper.ShowEquivalentPersonsOption && exportInputs.Workspace.IsUsingEquivalentPerson)
            {
                WordUtilities.UpdateHoursLabel(document);
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Open document begin");
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - FetchRepeatableBOEElement begin");
            }

			// Check the file for both the portrait and landscape special boe table elements
			SdtElement lastBOE = this.FetchRepeatableBOEElement(document);
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - FetchRepeatableBOEElement end");
            }

            SdtElement emptyBOE = null;

            // If this is the landscape template, create a copy of the BOE Table
            if (lastBOE != null)
            {
                emptyBOE = lastBOE.CloneNode(true) as SdtElement;
            }

            if (boeExportModelViews.Any() || boeSummaryGridModelViews.Any())
            {
                Collection<ResourceDTO> TravelResources = null;

                if (writelogstatements)
                {
                    this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - PopulateGeneralContent begin");
                }

                // Populate the workspace-level content in the template
                this.PopulateGeneralContent(document, boeExportModelViews.First());

                if (writelogstatements)
                {
                    this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - PopulateGeneralContent end");
                }

                // Iterate over all BOEs
                if (writelogstatements)
                {
                    this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Process BOE's begin");
                }

                // Check for MOQ RTE Templates for use later and clear the overrides to free up memory
                bool wsHasMoqRteTemplate = exportInputs.RTETemplatesOverrides.Any(x => x.SourceId == (int)RteTemplateSource.TaskMOQ);
                exportInputs.ClearRteOverrides();

                for (int ndx = 0; ndx < boeExportModelViews.Count; ndx++)
                {
                    Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> taskContainers = new Dictionary<BOEExportTaskElementType, BOEExportTaskContainer>();

                    BOEExportModelView boeExportModelView = boeExportModelViews.ElementAt(ndx);

                    if (TravelResources == null)
                    {
                        TravelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToCollection();
                    }

                    List<BOESummaryGridModelView> boeSummaryGridModelView = boeSummaryGridModelViews.Where(y => y.BOEID == boeExportModelView.BoeID).ToList();

                    try
                    {
                        // Populate the boe-level content in the template
                        this.PopulateBOEContent(lastBOE, boeExportModelView, boeSummaryGridModelView, exportInputs, document.MainDocumentPart, templateType, ref counters);

                        string useFontSizeDflt24 = this.GetFontSize(24, boeExportModelView.ExportFormat.TemplateType);
                        string useFontSizeDflt19 = this.GetFontSize(19, boeExportModelView.ExportFormat.TemplateType);
                        string useHeaderFontSizeDflt24 = this.GetHeaderFontSize(24, boeExportModelView.ExportFormat.TemplateType);
                        string useHeaderFontSizeDflt19 = this.GetHeaderFontSize(19, boeExportModelView.ExportFormat.TemplateType);
                        string useFont = this.GetFont("Times New Roman", boeExportModelView.ExportFormat.TemplateType);
                        
                        #region Populate the BOE hour summary table

                        if (boeExportModelView.IsMaterial)
                        {
                            SdtAlias TableAlias = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_HoursSummaryRollupContainer,
                                StringComparison.CurrentCultureIgnoreCase));

                            if (TableAlias != null)
                            {
								SdtElement element = TableAlias.Ancestors<SdtElement>().FirstOrDefault();
                                element.RemoveAllChildren();
                            }
                        }
                        else
                        {
                            SdtAlias TableAlias = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_HoursSummaryRollup,
                                                                                                 StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals(FieldName_HoursSummaryRollupWithResDesc));

                            if (TableAlias != null)
                            {
                                ICollection<BoeTaskElementDTO> CurrentTaskElements = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
                                Collection<BOEExportTaskElementLabor> currentLabors = boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToCollection();

                                Dictionary<int, List<LaborRollupByDate>> TaskRollup;
                                bool isNisscTemplate = TableAlias.Val.Value.Equals(FieldName_HoursSummaryRollupWithResDesc);

                                if (isNisscTemplate)
                                {
                                    this.ReplaceResNameForNISSC(currentLabors, exportInputs);
                                    TaskRollup = this.GetTaskHourRollupByCompanyAndLocation(CurrentTaskElements, currentLabors, boeExportModelView, exportInputs);
                                }
                                else
                                {
                                    TaskRollup = this.GetTaskHourRollup(CurrentTaskElements, currentLabors, boeExportModelView);
                                }

                                if (TaskRollup.Any())
                                {
                                    this.PopulateTaskElementRollup(TableAlias.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                        this.GetTaskDateRange(CurrentTaskElements, boeExportModelView), this.DefaultHoursFormat, null, useFont, useFontSizeDflt24, 
                                        useHeaderFontSizeDflt24, JustificationValues.Center, isNisscTemplate);
                                }
                                else
                                {
                                    SdtAlias TableAliasContainer = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_HoursSummaryRollupContainer, StringComparison.CurrentCultureIgnoreCase));

                                    if (TableAliasContainer != null)
                                    {
										SdtElement element = TableAliasContainer.Ancestors<SdtElement>().FirstOrDefault();
                                        element.RemoveAllChildren();
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Populate the BOE cost summary table

                        // Populate the boe cost summary table if it exists
                        SdtAlias TableAlias2 = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_CostSummaryRollup,
                                                                                              StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals(FieldName_LMSI_NISSCCostSummaryRollup,
                                                                                              StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals(FieldName_CostSummaryRollupWithResDesc));

                        if (TableAlias2 != null)
                        {
                            if (boeExportModelView.IsMaterial)
                            {
                                // never any material types with cost, remove
                                SdtAlias TableAliasContainer = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_CostSummaryRollupContainer, StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals("BOE:LMSI-NISSCCostSummaryRollupContainer", StringComparison.CurrentCultureIgnoreCase));

                                if (TableAliasContainer != null)
                                {
									SdtElement element = TableAliasContainer.Ancestors<SdtElement>().FirstOrDefault();
                                    element.RemoveAllChildren();
                                }
                            }
                            else
                            {
                                #region Labor costs

                                ICollection<BoeTaskElementDTO> laborTaskElementDtos = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                                if (TableAlias2.Val.Value.Equals(FieldName_LMSI_NISSCCostSummaryRollup, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    foreach (BoeTaskElementDTO dto in laborTaskElementDtos)
                                    {
                                        Collection<ResourceTypeDto> toRemove = new Collection<ResourceTypeDto>();

                                        foreach (ResourceTypeDto labor in dto.taskElementLabors)
                                        {
                                            if (labor.ResourceID != null)
                                            {
                                                ResourceDTO resource = exportInputs.ResourcesUsedInWsBoes.First(x => x.Id == (int)labor.ResourceID);

                                                if (resource.ElementOfCost == ElementOfCostType.LMLabor
                                                    || resource.ElementOfCost == ElementOfCostType.IWTA
                                                    || resource.ElementOfCost == ElementOfCostType.Sub)
                                                {
                                                    toRemove.Add(labor);
                                                }
                                            }
                                            else
                                            {
                                                toRemove.Add(labor);
                                            }
                                        }

                                        foreach (ResourceTypeDto remove in toRemove)
                                        {
                                            dto.taskElementLabors.Remove(remove);
                                        }
                                    }
                                }

                                Dictionary<int, List<LaborRollupByDateNew>> laborTaskCostRollup = this.GetGSMOLaborTaskCostRollup(laborTaskElementDtos, exportInputs, null);

                                #endregion

                                ICollection<OtherDirectCostDTO> CurrentODCElements = exportInputs.Odcs.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
                                Collection<BOEExportTaskElementLabor> CurrentODCLabors = boeExportModelView.TaskElements.Where(x => x.ElementType == BOEExportTaskElementType.ODC).SelectMany(x => x.taskElementLabors).ToCollection();
                                ICollection<TravelDTO> CurrentTravelElements = exportInputs.Travels.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                                if (TableAlias2.Val.Value.Equals(FieldName_LMSI_NISSCCostSummaryRollup)
                                    || TableAlias2.Val.Value.Equals(FieldName_CostSummaryRollupWithResDesc))
                                {
                                    this.ReplaceResNameForNISSC(CurrentODCLabors, exportInputs);
                                }

                                Dictionary<int, List<LaborRollupByDate>> ODCRollup = this.GetODCCostRollup(CurrentODCElements, CurrentODCLabors);

                                Dictionary<int, List<LaborRollupByDate>> TravelRollup = this.GetTravelCostRollup(CurrentTravelElements,
                                    boeExportModelView.TaskElements.Where(x => x.ElementType == BOEExportTaskElementType.Travel).ToCollection(),
                                    TravelResources);

                                foreach (int i in TravelRollup.Keys)
                                {
                                    if (ODCRollup.ContainsKey(i))
                                    {
                                        ODCRollup[i].AddRange(TravelRollup[i]);
                                    }
                                    else
                                    {
                                        ODCRollup.Add(i, TravelRollup[i]);
                                    }
                                }

                                foreach (int i in laborTaskCostRollup.Keys)
                                {
                                    if (ODCRollup.ContainsKey(i))
                                    {
                                        ODCRollup[i].AddRange(laborTaskCostRollup[i].ConvertToLaborRollupByDate());
                                    }
                                    else
                                    {
                                        ODCRollup.Add(i, laborTaskCostRollup[i].ConvertToLaborRollupByDate());
                                    }
                                }

                                if (ODCRollup.Any())
                                {
                                    this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), ODCRollup,
                                        this.GetODCTravelDateRange(CurrentODCElements, CurrentTravelElements), "C2", new NumberFormatInfo() { CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                        useFont, useFontSizeDflt19, useHeaderFontSizeDflt19, JustificationValues.Center);
                                }
                                else
                                {
                                    SdtAlias TableAliasContainer = lastBOE.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_CostSummaryRollupContainer, StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals("BOE:LMSI-NISSCCostSummaryRollupContainer", StringComparison.CurrentCultureIgnoreCase));

                                    if (TableAliasContainer != null)
                                    {
										SdtElement element = TableAliasContainer.Ancestors<SdtElement>().FirstOrDefault();
                                        element.RemoveAllChildren();
                                    }
                                }
                            }
                        }

                        #endregion

                        this.FetchContainerElements(lastBOE, taskContainers);

                        if (boeExportModelView.TaskElements.Any())
                        {
                            if (writelogstatements)
                            {
                                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Process Task Elements begin");
                            }

							// Removed the sorting by ElementType for taskelements. Requested by Frank - all exports now just sort by BOETaskElementOrder/ Whats shown on the users UI
							List<BOEExportTaskElement> OrderedTaskElements = boeExportModelView.TaskElements.OrderBy(y => y.BOETaskElementOrder).ThenBy(x => x.BOETaskElementID).ToList();

                            ICollection<BoeTaskElementDTO> taskElements = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                            #region Process all task elements

                            foreach (BOEExportTaskElement taskElement in OrderedTaskElements)
                            {
								// Get the container that will hold this task
								BOEExportTaskContainer taskContainer = this.GetTaskContainer(taskElement.ElementType, taskContainers);

                                if (taskContainer != null)
                                {
                                    // Populate the task-specific items in the template
                                    this.PopulateTaskElementContent(exportInputs, taskContainer, taskElement, ws, document.MainDocumentPart, wsHasMoqRteTemplate, ref counters);

                                    switch (taskElement.ElementType)
                                    {
                                        case BOEExportTaskElementType.None:
                                            break;

                                        #region Labor 

                                        case BOEExportTaskElementType.Labor:
                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_LaborHoursRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                BoeTaskElementDTO CurrentTaskElement = taskElements.FirstOrDefault(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> TaskRollup =
                                                    this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { CurrentTaskElement }, taskElement.taskElementLabors, boeExportModelView);

                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTaskElement.StartDate, CurrentTaskElement.EndDate), this.DefaultHoursFormat, null,
                                                    useFont, useFontSizeDflt24, useHeaderFontSizeDflt24, JustificationValues.Center);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_GSMOLaborHoursRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                BoeTaskElementDTO CurrentTaskElement = taskElements.FirstOrDefault(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<GSMOLaborRollupByDate>> TaskRollup = this.GetGSMOTaskHourRollup(new Collection<BoeTaskElementDTO> { CurrentTaskElement },
                                                    taskElement.taskElementLabors, true, exportInputs, boeExportModelView, false);
                                                string tempFontSize = (boeExportModelView.ExportFormat.TemplateType == ExcelReportTemplateType.LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES) ? "22" : "24";

                                                this.PopulateGSMOTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTaskElement.StartDate, CurrentTaskElement.EndDate), this.DefaultHoursFormat, null,
                                                    "Times New Roman", tempFontSize, templateType);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_GSMOLaborHoursRollupNoResTotals,
                                                StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                BoeTaskElementDTO CurrentTaskElement = taskElements.FirstOrDefault(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<GSMOLaborRollupByDate>> TaskRollup = this.GetGSMOTaskHourRollup(new Collection<BoeTaskElementDTO> { CurrentTaskElement },
                                                    taskElement.taskElementLabors, false, exportInputs, boeExportModelView, false);

                                                this.PopulateGSMOTaskElementRollupWithoutResTotals(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTaskElement.StartDate, CurrentTaskElement.EndDate), this.DefaultHoursFormat, null,
                                                    "Times New Roman", "20");
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_SMORSLaborHoursRollup,
                                                                                                                             StringComparison.CurrentCultureIgnoreCase) || s.Val.Value.Equals(FieldName_SMORSLaborHoursRollupWithPerfOrg,
                                                                                                                             StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                BoeTaskElementDTO CurrentTaskElement = taskElements.FirstOrDefault(x => x.Id == taskElement.BOETaskElementID.Value);

                                                bool includePerfOrg = TableAlias2.Val.Value.Contains("PerfOrg");

                                                //we can just use the GSMO task rollup for the SMORS template
                                                Dictionary<int, List<GSMOLaborRollupByDate>> TaskRollup = this.GetGSMOTaskHourRollup(new Collection<BoeTaskElementDTO> { CurrentTaskElement },
                                                    taskElement.taskElementLabors, false, exportInputs, boeExportModelView, includePerfOrg);

                                                this.PopulateSMORSTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTaskElement.StartDate, CurrentTaskElement.EndDate), this.DefaultHoursFormat, null,
                                                    "Times New Roman", "20", includePerfOrg);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MSTLaborHoursRollup,
                                                StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                BoeTaskElementDTO CurrentTaskElement = taskElements.FirstOrDefault(x => x.Id == taskElement.BOETaskElementID.Value);

                                                if (CurrentTaskElement != null)
                                                {
                                                    Collection<LaborRollupByDate> TaskRollup = this.GetRollupByYearData(new List<BoeTaskElementDTO>() { CurrentTaskElement }).ToCollection();

                                                    if (TaskRollup != null && TaskRollup.Any())
                                                    {
														SdtElement element = TableAlias2.Ancestors<SdtElement>().FirstOrDefault();
                                                        this.PopulateBoeYearSummaryRollup(element, TaskRollup, this.DefaultHoursFormat, null, useFont, "18", "18", TableAlias2.Val.Value, JustificationValues.Center);
                                                    }
                                                }
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MSTLaborCostRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                Collection<LaborRollupByDate> Rollup = null;
                                                ICollection<BoeTaskElementDTO> LaborElements = exportInputs.TaskElements.Where(x => x.Id == taskElement.BOETaskElementID).ToList();

                                                if (LaborElements.Any())
                                                {
                                                    DateRange dateRange = this.GetTaskDateRange(LaborElements.ToList(), boeExportModelView);
                                                    Rollup = this.GetLaborCostSummaryRollupByYearData(LaborElements.ToList(), dateRange);
                                                }

                                                this.PopulateTaskCostRollup(Rollup, useFont, TableAlias2, taskContainer, FieldName_MSTLaborCostRollupContainer);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_TaskCustomFields, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                SdtElement laborCustomFieldsElement = TableAlias2.Ancestors<SdtElement>().FirstOrDefault();

                                                if (laborCustomFieldsElement != null)
                                                {
                                                    if (taskElement.BOETaskElementID.HasValue)
                                                    {
                                                        Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
                                                        if (exportInputs.TaskElementsMappingWithCustomFieldsValuesAndContainerIds.Any(x => x.Key == taskElement.BOETaskElementID.Value))
                                                        { customFieldValueMappings.Add(taskElement.BOETaskElementID.Value, exportInputs.TaskElementsMappingWithCustomFieldsValuesAndContainerIds.FirstOrDefault(x => x.Key == taskElement.BOETaskElementID.Value).Value); }

                                                        IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> laborTaskCustomFields = this.GetTaskElementCustomFields(customFieldValueMappings, exportInputs);

                                                        this.PopulateCustomFields(laborCustomFieldsElement, laborTaskCustomFields);
                                                    }
                                                    else
                                                    {
                                                        laborCustomFieldsElement.RemoveIt();
                                                    }
                                                }
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceContainer + "-Labor"));

                                            if (TableAlias2 != null)
                                            {
                                                Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> ResourceContainers = new Dictionary<BOEExportTaskElementType, BOEExportTaskContainer>();
                                                this.FetchResourceContainerElements(taskContainer.TaskContainer, ResourceContainers);
                                                ICollection<ResourceTypeDto> resourceTypeDtos = taskElements.Where(t => t.BOETaskID == taskElement.BOETaskID).SelectMany(r => r.taskElementLabors).ToList();

												List<BOEExportTaskElementLabor> orderedResources;
												if (Utilities.IsBRCEnabledForSystem)
												{
													orderedResources = taskElement.taskElementLabors
													.Where(x => x.ExportFields.ContainsKey(FieldName_BrcID)
																&& x.ExportFields.ContainsKey(FieldName_ResourceID)
																&& x.ExportFields.ContainsKey(FieldName_LaborTypeID)
																&& x.ExportFields.ContainsKey(FieldName_PerfOrg))
													.OrderBy(o => o.ExportFields[FieldName_BrcID])
													.ThenByDescending(t => t.ExportFields[FieldName_ResourceID])
													.ThenBy(p => p.ExportFields[FieldName_PerfOrg]).ToList();
												}
												else
												{
													orderedResources = taskElement.taskElementLabors
													.Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID)
																&& x.ExportFields.ContainsKey(FieldName_LaborTypeID)
																&& x.ExportFields.ContainsKey(FieldName_PerfOrg))
													.OrderBy(o => o.ExportFields[FieldName_ResourceID])
													.ThenBy(t => t.ExportFields[FieldName_PerfOrg]).ToList();
												}

                                                foreach (BOEExportTaskElementLabor resource in orderedResources)
                                                {
													BOEExportTaskContainer resourceContainer = this.GetTaskContainer(taskElement.ElementType, ResourceContainers);
                                                    PerformingOrgDTO currentPerfOrg = exportInputs.PerformingOrgsForWsList
                                                        .FirstOrDefault(p => p.PerformingOrgName == resource.ExportFields[FieldName_PerfOrg]);

                                                    if (currentPerfOrg != null)
                                                    {
                                                        ResourceTypeDto currentResourceTypeDto = resourceTypeDtos
                                                            .FirstOrDefault(r => r.ResourceID.ToString() == resource.ExportFields[FieldName_ResourceID]
                                                                                 && r.Id.ToString() == resource.ExportFields[FieldName_LaborTypeID]
                                                                                 && r.PerformingOrgID == currentPerfOrg.Id
                                                                                 && r.StartDate == resource.StartDate && r.EndDate == resource.EndDate);

                                                        if (resourceContainer != null && currentResourceTypeDto != null)
                                                        {
                                                            this.PopulateResourceContent(resourceContainer, resource);

                                                            SdtAlias TableAlias3 = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResCustomFields));

                                                            if (TableAlias3 != null)
                                                            {
                                                                SdtElement resourceCustomFieldsElement = TableAlias3.Ancestors<SdtElement>().FirstOrDefault();
                                                                if (resourceCustomFieldsElement != null)
                                                                {
                                                                    if (resource.ExportFields.ContainsKey(FieldName_ResourceID))
                                                                    {
                                                                        int laborTypeID = Convert.ToInt32(resource.ExportFields[FieldName_LaborTypeID]);

                                                                        Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                                                                        KeyValuePair<int, ICollection<KeyValuePair<int, int>>> laborMapping = exportInputs.LaborTypesMappingWithCustomFieldsValuesAndContainerIds.FirstOrDefault(x => x.Key == laborTypeID);
                                                                        if (!laborMapping.Equals(default(KeyValuePair<int, ICollection<KeyValuePair<int, int>>>)))
                                                                        {
                                                                            customFieldValueIdMappings.Add(laborTypeID, laborMapping.Value);
                                                                        }

                                                                        IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> laborTaskCustomFields = this.GetResourceCustomFields(customFieldValueIdMappings, exportInputs);

                                                                        this.PopulateCustomFields(resourceCustomFieldsElement, laborTaskCustomFields);
                                                                    }
                                                                    else
                                                                    {
                                                                        resourceCustomFieldsElement.RemoveIt();
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                TableAlias3.RemoveIt();
                                                            }

                                                            TableAlias3 = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceHoursRollup));
                                                            if (TableAlias3 != null && currentResourceTypeDto.SpreadType.Equals(SpreadType.Hours))
                                                            {
                                                                Collection<LaborRollupByDate> rollup = this.GetResourceRollup(currentResourceTypeDto, taskElement.taskElementLabors).ToCollection();

																SdtElement element = TableAlias3.Ancestors<SdtElement>().FirstOrDefault();
                                                                if (element != null)
                                                                {
                                                                    this.PopulateBoeYearSummaryRollup(element, rollup, this.DefaultHoursFormat, null, useFont, "18", "18", TableAlias3.Val.Value, JustificationValues.Center);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                SdtAlias TableAlias3a = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceHoursRollupContainer));

                                                                if (TableAlias3a != null)
                                                                {
																	SdtElement element = TableAlias3a.Ancestors<SdtElement>().FirstOrDefault();
                                                                    if (element != null)
                                                                    {
                                                                        element.RemoveAllChildren();
                                                                    }
                                                                }

                                                                TableAlias3a.RemoveIt();
                                                            }

                                                            TableAlias3 = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceCostRollup));

                                                            if (TableAlias3 != null && currentResourceTypeDto.SpreadType.Equals(SpreadType.Cost))
                                                            {
                                                                Collection<LaborRollupByDate> rollup = this.GetResourceRollup(currentResourceTypeDto, taskElement.taskElementLabors).ToCollection();

                                                                this.PopulateTaskCostRollup(rollup, useFont, TableAlias3, resourceContainer, FieldName_ResourceCostRollupContainer);
                                                            }
                                                            else
                                                            {
                                                                SdtAlias TableAlias3a = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceCostRollupContainer));

                                                                if (TableAlias3a != null)
                                                                {
																	SdtElement element = TableAlias3a.Ancestors<SdtElement>().FirstOrDefault();
                                                                    if (element != null)
                                                                    {
                                                                        element.RemoveAllChildren();
                                                                    }
                                                                }

                                                                TableAlias3a.RemoveIt();
                                                            }
                                                        }
                                                    }
                                                }

                                                this.CleanEmptyTaskContainers(ResourceContainers);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_SOWResourceTable));

                                            if (TableAlias2 != null)
                                            {
                                                ICollection<ResourceTypeDto> resourceTypeDtos = taskElements.Where(x => x.BOETaskID == taskElement.BOETaskID).SelectMany(y => y.taskElementLabors).ToCollection();
                                                this.PopulateSOWResourceTable(exportInputs, TableAlias2, boeExportModelView, resourceTypeDtos);
                                            }

                                            break;

                                        #endregion

                                        #region Travel

                                        case BOEExportTaskElementType.Travel:
                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_TravelDirectCostRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                TravelDTO CurrentTravel = exportInputs.Travels.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> TaskRollup = this.GetTravelCostRollup(new Collection<TravelDTO> { CurrentTravel }, new Collection<BOEExportTaskElement> { taskElement }, TravelResources);
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTravel.StartDate, CurrentTravel.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, useFontSizeDflt19, useHeaderFontSizeDflt19, JustificationValues.Center);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_GSMOTravelDirectCostRollup,
                                                StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                TravelDTO CurrentTravel = exportInputs.Travels.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> TaskRollup = this.GetTravelCostRollup(new Collection<TravelDTO> { CurrentTravel }, new Collection<BOEExportTaskElement> { taskElement }, TravelResources);
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentTravel.StartDate, CurrentTravel.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, "20", "20", JustificationValues.Right);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MSTTravelCostRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                DateRange dateRange = null;
                                                Collection<LaborRollupByDate> Rollup = null;
                                                ICollection<TravelDTO> TravelElements = exportInputs.Travels.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                                                if (TravelElements.Any())
                                                {
                                                    dateRange = this.GetODCTravelDateRange(null, TravelElements); // get just travel range
                                                    Rollup = this.GetODCTravelCostSummaryRollupByYearData(new List<OtherDirectCostDTO>(), boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToCollection(), dateRange); // not using ODC, but empty ICollection needed (null causes error)
                                                }

                                                this.PopulateTaskCostRollup(Rollup, useFont, TableAlias2, taskContainer, FieldName_MSTTravelCostRollupContainer);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ZoneTravelSummary, StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                SdtElement zoneElement = TableAlias2.Ancestors<SdtElement>().FirstOrDefault();
                                                SdtElement zoneTableElement = WordUtilities.GetTaggedChildElement(zoneElement, FieldName_ZoneTravelSummaryTable);
                                                SdtElement multiLabel = WordUtilities.GetTaggedChildElement(zoneTableElement, FieldName_MultiLabel);
                                                Collection<BOEExportTaskElementLabor> zoneLabors = taskElement.taskElementLabors.Where(x => x.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.ZoneNoAirfare.ToDescription() || x.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.ZoneAirfare.ToDescription()).ToCollection();
                                                if (zoneLabors.Any())
                                                {
                                                    if (boeExportModelView.IsMultiClinWbs && multiLabel != null)
                                                    {
                                                        SdtElement currentTable = zoneTableElement;
														IEnumerable<IGrouping<string, BOEExportTaskElementLabor>> multiGroups = zoneLabors.GroupBy(x => x.ExportFields[FieldName_MultiLabel]);
                                                        foreach (IGrouping<string, BOEExportTaskElementLabor> group in multiGroups)
                                                        {
                                                            SdtElement clonedTable = zoneTableElement.CloneNode(true) as SdtElement;
                                                            SdtAlias currentRowAlias = clonedTable.Descendants<SdtAlias>().LastOrDefault(a => a.Val.Value == FieldName_TaskTypeRow_ZoneTravel);
                                                            TableRow originalRow = currentRowAlias.Ancestors<TableRow>().FirstOrDefault();
                                                            TableRow currentRow = originalRow;
                                                            multiLabel = WordUtilities.GetTaggedChildElement(clonedTable, FieldName_MultiLabel);
                                                            WordUtilities.SetElementText(multiLabel, group.First().ExportFields[FieldName_MultiLabel]);
                                                            foreach (BOEExportTaskElementLabor labor in group)
                                                            {
                                                                if (labor.ExportFields.ContainsKey(FieldName_TaskTypeTravelMode) && labor.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.ZoneAirfare.ToDescription())
                                                                {
                                                                    // Per Diem
                                                                    this.PopulateRMSTravelSummaryTableRow(labor, originalRow, currentRow);

                                                                    // Airfare
                                                                    labor.ExportFields[FieldName_ResourceName] = labor.ExportFields[FieldName_SecondaryResourceName];
                                                                    labor.ExportFields[FieldName_TaskTypeDays] = string.Empty;
                                                                    this.PopulateRMSTravelSummaryTableRow(labor, originalRow, currentRow);
                                                                }
                                                                else
                                                                {
                                                                    this.PopulateRMSTravelSummaryTableRow(labor, originalRow, currentRow);
                                                                }
                                                            }

                                                            originalRow.RemoveIt();
                                                            currentTable.InsertAfterSelf(clonedTable);
                                                            currentTable = clonedTable;
                                                        }

                                                        zoneTableElement.RemoveIt();
                                                    }
                                                    else
                                                    {
                                                        multiLabel.RemoveIt();

                                                        SdtAlias zoneRowAlias = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(a => a.Val.Value == FieldName_TaskTypeRow_ZoneTravel);
                                                        TableRow zoneRow = zoneRowAlias.Ancestors<TableRow>().FirstOrDefault();
                                                        TableRow currentRow = zoneRow;

                                                        foreach (BOEExportTaskElementLabor labor in zoneLabors)
                                                        {
                                                            if (labor.ExportFields.ContainsKey(FieldName_TaskTypeTravelMode) && labor.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.ZoneAirfare.ToDescription())
                                                            {
                                                                // Per Diem
                                                                this.PopulateRMSTravelSummaryTableRow(labor, zoneRow, currentRow);

                                                                // Airfare
                                                                labor.ExportFields[FieldName_ResourceName] = labor.ExportFields[FieldName_SecondaryResourceName];
                                                                labor.ExportFields[FieldName_TaskTypeDays] = string.Empty;
                                                                this.PopulateRMSTravelSummaryTableRow(labor, zoneRow, currentRow);
                                                            }
                                                            else
                                                            {
                                                                this.PopulateRMSTravelSummaryTableRow(labor, zoneRow, currentRow);
                                                            }
                                                        }

                                                        zoneRow.RemoveIt();
                                                    }
                                                }
                                                else
                                                {
                                                    zoneElement.RemoveIt();
                                                }
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_NonzoneTravelSummary, StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                SdtElement nonzoneElement = TableAlias2.Ancestors<SdtElement>().FirstOrDefault();
                                                SdtElement nonzoneTableElement = WordUtilities.GetTaggedChildElement(nonzoneElement, FieldName_NonzoneTravelSummaryTable);
                                                SdtElement multiLabel = WordUtilities.GetTaggedChildElement(nonzoneTableElement, FieldName_MultiLabel);
                                                Collection<BOEExportTaskElementLabor> nonzoneLabors = taskElement.taskElementLabors.Where(x => x.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.NonZoneDomestic.ToDescription() || x.ExportFields[FieldName_TaskTypeTravelMode] == MSTTravelMode.NonZoneInternational.ToDescription()).ToCollection();
                                                if (nonzoneLabors.Any())
                                                {
                                                    if (boeExportModelView.IsMultiClinWbs && multiLabel != null)
                                                    {
                                                        SdtElement currentTable = nonzoneTableElement;
														IEnumerable<IGrouping<string, BOEExportTaskElementLabor>> multiGroups = nonzoneLabors.GroupBy(x => x.ExportFields[FieldName_MultiLabel]);
                                                        foreach (IGrouping<string, BOEExportTaskElementLabor> group in multiGroups)
                                                        {
                                                            SdtElement clonedTable = nonzoneTableElement.CloneNode(true) as SdtElement;
                                                            SdtAlias currentRowAlias = clonedTable.Descendants<SdtAlias>().LastOrDefault(a => a.Val.Value == FieldName_TaskTypeRow_NonzoneTravel);
                                                            TableRow originalRow = currentRowAlias.Ancestors<TableRow>().FirstOrDefault();
                                                            TableRow currentRow = originalRow;
                                                            multiLabel = WordUtilities.GetTaggedChildElement(clonedTable, FieldName_MultiLabel);
                                                            WordUtilities.SetElementText(multiLabel, group.First().ExportFields[FieldName_MultiLabel]);
                                                            foreach (BOEExportTaskElementLabor labor in group)
                                                            {
                                                                this.PopulateRMSTravelSummaryTableRow(labor, originalRow, currentRow);
                                                            }

                                                            originalRow.RemoveIt();
                                                            currentTable.InsertAfterSelf(clonedTable);
                                                            currentTable = clonedTable;
                                                        }

                                                        nonzoneTableElement.RemoveIt();
                                                    }
                                                    else
                                                    {
                                                        multiLabel.RemoveIt();

                                                        SdtAlias nonzoneRowAlias = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(a => a.Val.Value == FieldName_TaskTypeRow_NonzoneTravel);
                                                        TableRow nonzoneRow = nonzoneRowAlias.Ancestors<TableRow>().FirstOrDefault();
                                                        TableRow currentRow = nonzoneRow;

                                                        foreach (BOEExportTaskElementLabor labor in nonzoneLabors)
                                                        {
                                                            this.PopulateRMSTravelSummaryTableRow(labor, nonzoneRow, currentRow);
                                                        }

                                                        nonzoneRow.RemoveIt();
                                                    }
                                                }
                                                else
                                                {
                                                    nonzoneElement.RemoveIt();
                                                }
                                            }

                                            break;

                                        #endregion

                                        #region ODC

                                        case BOEExportTaskElementType.ODC:

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ODCDirectCostRollup, StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                OtherDirectCostDTO CurrentODC = exportInputs.Odcs.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> TaskRollup = this.GetODCCostRollup(new Collection<OtherDirectCostDTO>() { CurrentODC }, taskElement.taskElementLabors);
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentODC.StartDate, CurrentODC.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, useFontSizeDflt19, useHeaderFontSizeDflt19, JustificationValues.Center);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_GSMOODCDirectCostRollup,
                                                StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                OtherDirectCostDTO CurrentODC = exportInputs.Odcs.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> TaskRollup = this.GetODCCostRollup(new Collection<OtherDirectCostDTO>() { CurrentODC }, taskElement.taskElementLabors);
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), TaskRollup,
                                                    new DateRange(CurrentODC.StartDate, CurrentODC.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, "20", "20", JustificationValues.Right);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MSTODCCostRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                Collection<LaborRollupByDate> Rollup = null;
                                                ICollection<OtherDirectCostDTO> ODCElements = exportInputs.Odcs.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                                                if (ODCElements.Any())
                                                {
                                                    DateRange dateRange = this.GetODCTravelDateRange(ODCElements, null); //get just ODC range
                                                    Rollup = this.GetODCTravelCostSummaryRollupByYearData(ODCElements, new Collection<BOEExportTaskElementLabor>(), dateRange); // get rollup just for odc, not using travel, but empty collection needed (null causes error)
                                                }

                                                this.PopulateTaskCostRollup(Rollup, useFont, TableAlias2, taskContainer, FieldName_MSTODCCostRollupContainer);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceContainer + "-ODC"));
                                            if (TableAlias2 != null)
                                            {
                                                Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> ResourceContainers = new Dictionary<BOEExportTaskElementType, BOEExportTaskContainer>();
                                                this.FetchResourceContainerElements(taskContainer.TaskContainer, ResourceContainers);
                                                ICollection<OtherDirectCostType> ODCTypes = exportInputs.Odcs.Where(x => x.TaskID == taskElement.BOETaskID).SelectMany(o => o.ODCTypes).ToList();

												List<BOEExportTaskElementLabor> orderedResources = taskElement.taskElementLabors
                                                    .Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID) && x.ExportFields.ContainsKey(FieldName_PerfOrg))
                                                    .OrderBy(y => y.ExportFields[FieldName_ResourceID])
                                                    .ThenBy(z => z.ExportFields[FieldName_PerfOrg]).ToList();

                                                foreach (BOEExportTaskElementLabor resource in orderedResources)
                                                {
                                                    // taskElement.taskElementLabors groups resources with the same performing org, but we need to output them separately
                                                    PerformingOrgDTO currentPerfOrg = exportInputs.PerformingOrgsForWsList.FirstOrDefault(p => p.PerformingOrgName == resource.ExportFields[FieldName_PerfOrg]);

                                                    if (currentPerfOrg != null)
                                                    {
                                                        ICollection<OtherDirectCostType> currentODCTypes = ODCTypes.Where(r =>
                                                            r.ResourceID.ToString() == resource.ExportFields[FieldName_ResourceID]
                                                            && r.PerformingOrgID == currentPerfOrg.Id).ToList();

                                                        if (currentODCTypes != null)
                                                        {
															List<OtherDirectCostType> orderedCurrentODCTypes = currentODCTypes.OrderBy(x => x.StartDate).ToList();

                                                            foreach (OtherDirectCostType odcType in orderedCurrentODCTypes)
                                                            {
																BOEExportTaskContainer resourceContainer = this.GetTaskContainer(taskElement.ElementType, ResourceContainers);

                                                                if (resourceContainer != null)
                                                                {
                                                                    // set resource start/end dates since they are null originally
                                                                    resource.StartDate = odcType.StartDate;
                                                                    resource.EndDate = odcType.EndDate;
                                                                    resource.ExportFields[FieldName_GenBOEResourceID] = odcType.ODCTypeID.ToString();

                                                                    this.PopulateResourceContent(resourceContainer, resource);

                                                                    SdtAlias TableAlias3 = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceCostRollup));
                                                                    if (TableAlias3 != null)
                                                                    {
                                                                        Collection<LaborRollupByDate> rollup = this.GetODCResourceRollup(odcType, taskElement.taskElementLabors).ToCollection();

                                                                        this.PopulateTaskCostRollup(rollup, useFont, TableAlias3, resourceContainer, FieldName_ResourceCostRollupContainer);
                                                                    }
                                                                    else
                                                                    {
                                                                        SdtAlias TableAlias3a = resourceContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceCostRollupContainer));
                                                                        if (TableAlias3a != null)
                                                                        {
																			SdtElement element = TableAlias3a.Ancestors<SdtElement>().FirstOrDefault();
                                                                            if (element != null)
                                                                            {
                                                                                element.RemoveAllChildren();
                                                                            }
                                                                        }

                                                                        TableAlias3a.RemoveIt();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                this.CleanEmptyTaskContainers(ResourceContainers);
                                            }
                                            break;

                                        #endregion

                                        #region Material

                                        case BOEExportTaskElementType.Material:
                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MaterialDirectCostRollup, StringComparison.CurrentCultureIgnoreCase));

                                            if (TableAlias2 != null)
                                            {
                                                MaterialDTO CurrentMaterialElement = exportInputs.Materials.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> MaterialRollup = new Dictionary<int, List<LaborRollupByDate>>();
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), MaterialRollup,
                                                    new DateRange(CurrentMaterialElement.StartDate, CurrentMaterialElement.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = 1, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, useFontSizeDflt19, useHeaderFontSizeDflt19, JustificationValues.Center);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_GSMOMaterialDirectCostRollup,
                                                StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                MaterialDTO CurrentMaterialElement = exportInputs.Materials.First(x => x.Id == taskElement.BOETaskElementID.Value);

                                                Dictionary<int, List<LaborRollupByDate>> MaterialRollup = new Dictionary<int, List<LaborRollupByDate>>();
                                                this.PopulateTaskElementRollup(TableAlias2.Ancestors<SdtElement>().FirstOrDefault(), MaterialRollup,
                                                    new DateRange(CurrentMaterialElement.StartDate, CurrentMaterialElement.EndDate), "C2", new NumberFormatInfo() { CurrencyNegativePattern = 1, CurrencySymbol = string.Empty, CurrencyDecimalDigits = 2 },
                                                    useFont, "20", "20", JustificationValues.Right);
                                            }

                                            TableAlias2 = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(FieldName_MSTMaterialCostRollup, StringComparison.CurrentCultureIgnoreCase));
                                            if (TableAlias2 != null)
                                            {
                                                Collection<LaborRollupByDate> Rollup = null;
                                                ICollection<MaterialDTO> MaterialElements = exportInputs.Materials.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                                                if (MaterialElements.Any())
                                                {
                                                    Rollup = new Collection<LaborRollupByDate>();
                                                }

                                                this.PopulateTaskCostRollup(Rollup, useFont, TableAlias2, taskContainer, FieldName_MSTMaterialCostRollupContainer);
                                            }
                                            break;

                                        #endregion

                                        default:
                                            break;
                                    }
                                }
                            }

                            #endregion

                            if (writelogstatements)
                            {
                                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Process Task Elements end");
                            }
                        }

                        this.CleanEmptyTaskContainers(taskContainers);

                        #region Prepare output for next BOE

                        // If this was not the last BOE, add new BOE fields to the template. The fields that
                        // are added are different based on whether this is a landscape or portrait template.
                        if (lastBOE != null && ndx != boeExportModelViews.Count - 1)
                        {
							// Clone new table from the empty table template
							SdtElement newBOE = emptyBOE.CloneNode(true) as SdtElement;

							// Add a page break before the next BOE
							Paragraph pageBreak = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                            lastBOE.InsertAfterSelf(pageBreak);

                            // Add the new empty table to the file after the last table
                            pageBreak.InsertAfterSelf(newBOE);

                            // Reset the last table to be the newly inserted empty table
                            lastBOE = newBOE;
                        }

                        #endregion
                    }
                    catch (GenValidationException) { throw; } // allow the controller to handle this in a nicer way.
                    catch (Exception ex)
                    {
                        // throw new exception with more info added
                        throw new GeneralAppException(
                            string.Format("Error encountered while exporting BOE-{0} WBS {1} {2} CLIN {3} {4}: {5}",
                                boeExportModelView.BoeID, boeExportModelView.WBSNumber, boeExportModelView.WBSTitle,
                                boeExportModelView.CLINNumber, boeExportModelView.CLINTitle, ex.Message),
                            ex);
                    }
                }

                if (writelogstatements)
                {
                    this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Process BOE's end");
                }
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ExportBOEToWordFile - Open document end");
            }

            this.DoFinalDocumentCleanup(document);
        }


        /// <summary>
        /// This function will populate the BOEExportModelViews based on the BOE DTOs.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// The BOE Export ModelViews.
        /// </returns>
        public ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(BOEExportInputs exportInputs)
        {
            return this.exportConverter.ConvertBoeDTOsToExportMVs(exportInputs);
        }

        /// <summary>
        /// Exports the BOE Import Template with the correct dropdowns populated.
        /// </summary>
        /// <param name="templateFileLocation">Template file location.</param>
        /// <param name="workspace">Full Workspace.</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        /// <returns>Saved Excel file location.</returns>
        public string ExportTemplate(string templateFileLocation, FullWorkspace workspace, bool blankTemplate)
        {
            return this.ExportToExcelFile(templateFileLocation, workspace, blankTemplate);
        }

        /// <summary>
        /// Exports the BOEs to Excel.
        /// </summary>
        /// <param name="templateFileLocation">Template file location.</param>
        /// <param name="workspace">Full Workspace.</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        /// <returns>Saved Excel file location.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace workspace, bool blankTemplate)
        {
            if (workspace == null) { throw new ArgumentNullException(nameof(workspace)); }

            Collection<WbsDTO> allWbs = workspace.WbsElements.ToCollection<WbsDTO>();
            Collection<ClinDTO> allClins = workspace.Clins.ToCollection<ClinDTO>();
            Collection<PermissionsDTO> permissionsForWorkspace = this.permissionsDTOLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);
            HashSet<UserDTO> allUsersWithPotentialPermissions = new HashSet<UserDTO>(this._IUserDTODataLoader.GetByIds(permissionsForWorkspace.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

            List<int> boeIds = workspace.Boes.Select(x => x.Id).ToList();
            HashSet<PermissionsDTO> rolesWithBoes = new HashSet<PermissionsDTO>(this.permissionsDTOLoader.GetBOEPermissions(boeIds).Where(x => x.BOEId.HasValue).ToCollection());

            // Get users broken down by roles, for the boes
            HashSet<UserDTO> allBoeUsers = new HashSet<UserDTO>(this._IUserDTODataLoader.GetByIds(rolesWithBoes.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

            HashSet<int> authorIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.Author).Select(x => x.ETIUserId).ToCollection());
            Collection<UserDTO> allAuthors = allUsersWithPotentialPermissions.Where(u => authorIds.Contains(u.UserID)).OrderBy(x => x.DisplayName).ToCollection<UserDTO>();

            HashSet<int> subcontractorAuthorIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x.ETIUserId).ToCollection());
            Collection<UserDTO> allSubcontratorAuthors = allUsersWithPotentialPermissions.Where(x => subcontractorAuthorIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).ToCollection();

            HashSet<int> approverIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).ToCollection());
            Collection<UserDTO> allApprovers = allUsersWithPotentialPermissions.Where(x => approverIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).ToCollection();

            #region Remove AD Groups from users

            allAuthors = this.CleanupAdGroupsFromUsers(allAuthors);
            allSubcontratorAuthors = this.CleanupAdGroupsFromUsers(allSubcontratorAuthors);
            allApprovers = this.CleanupAdGroupsFromUsers(allApprovers);

            #endregion

            Collection<string> allMaterials = new Collection<string> { "Yes", "No" };
            Collection<string> allMultiBOE = new Collection<string> { "Yes", "No" };

            HashSet<BOEStateModelView> allStates = new HashSet<BOEStateModelView>(this.commonDataMapper.getBOEStates().Where(b => b.BOEStateID != (int)BOEState.DateShiftDraft && b.BOEStateID != (int)BOEState.DraftLocked));
            Collection<string> allStatus = new Collection<string>(allStates.Select(x => x.BOEState).ToList());
            allStatus.Add("Delete");

			#region Create Options tab

			// Create collections of strings for each row in the export file
			ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet("Options Lists");

            int maxRows = allWbs.Count > allClins.Count ? allWbs.Count : allClins.Count;
            maxRows = maxRows > allAuthors.Count ? maxRows : allAuthors.Count;
            maxRows = maxRows > allApprovers.Count ? maxRows : allApprovers.Count;
            maxRows = maxRows > allStatus.Count ? maxRows : allStatus.Count;
            maxRows = maxRows > allMaterials.Count ? maxRows : allMaterials.Count;
            maxRows = maxRows > allMultiBOE.Count ? maxRows : allMultiBOE.Count;
            maxRows = maxRows > allSubcontratorAuthors.Count ? maxRows : allSubcontratorAuthors.Count;

            string toReturn = string.Empty;

            for (int i = 0; i < maxRows; i++)
            {
                optionsListWorksheet.Add(new Collection<string>
                {
                    allWbs.ElementAtOrDefault(i) != null ? allWbs[i].WbsString : string.Empty,
                    allClins.ElementAtOrDefault(i) != null ? allClins[i].ClinString : string.Empty,
                    allAuthors.ElementAtOrDefault(i) != null ? allAuthors[i].DisplayName : string.Empty,
                    allApprovers.ElementAtOrDefault(i) != null ? allApprovers[i].DisplayName : string.Empty,
                    allStatus.ElementAtOrDefault(i) != null ? allStatus[i] : string.Empty,
                    allMaterials.ElementAtOrDefault(i) != null ? allMaterials[i] : string.Empty,
                    allSubcontratorAuthors.ElementAtOrDefault(i) != null ? allSubcontratorAuthors[i].DisplayName : string.Empty,
                    allMultiBOE.ElementAtOrDefault(i) != null ? allMultiBOE[i] : string.Empty
                });
            }

			#endregion

			ExcelExportWorksheet firstWorksheet = new ExcelExportWorksheet();

            if (workspace.Boes.Any() && !blankTemplate)
            {
                foreach (FullBoe boe in workspace.Boes)
                {
                    HashSet<int> boeAuthorIds = new HashSet<int>(rolesWithBoes.Where(x => x.Role == Role.Author && x.BOEId == boe.Id).Select(x => x.ETIUserId).ToCollection());
                    Collection<string> authors = allBoeUsers.Where(x => boeAuthorIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).Select(x => x.DisplayName).ToCollection();

                    HashSet<int> boeSubcontractorAuthorIds = new HashSet<int>(rolesWithBoes.Where(x => x.Role == Role.SubcontractorAuthor && x.BOEId == boe.Id).Select(x => x.ETIUserId).ToCollection());
                    Collection<string> subcontractorAuthors = allBoeUsers.Where(x => boeSubcontractorAuthorIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).Select(x => x.DisplayName).ToCollection();

                    HashSet<int> boeApproverIds = new HashSet<int>(rolesWithBoes.Where(x => x.Role == Role.Approver && x.BOEId == boe.Id).Select(x => x.ETIUserId).ToCollection());
                    Collection<string> approvers = allBoeUsers.Where(x => boeApproverIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).Select(x => x.DisplayName).ToCollection();

                    string authorString = string.Join("\n", authors);
                    string subcontractorAuthorString = string.Join("\n", subcontractorAuthors);
                    string approverString = string.Join("\n", approvers);

                    WbsDTO wbs = allWbs.FirstOrDefault(x => x.Id == boe.WBSID);
                    ClinDTO clin = allClins.FirstOrDefault(x => x.Id == boe.CLINID);
                    string state = allStates.Where(x => x.BOEStateID == (int)boe.State).Select(x => x.BOEState).FirstOrDefault();
                    if (boe.State == BOEState.DraftLocked || boe.State == BOEState.DateShiftDraft)
                    {
                        state = allStates.FirstOrDefault(s => s.BOEStateID == (int)BOEState.Draft).BOEState;
                    }

                    firstWorksheet.Add(new Collection<string>
                    {
                        boe.Id.ToString(),
                        wbs != null ? wbs.WbsString : string.Empty,
                        boe.Title,
                        clin != null ? clin.ClinString : string.Empty,
                        boe.IsMultiClinWbs ? "Yes" : "No",
                        boe.isMaterial ? "Yes" : "No",
                        authorString,
                        subcontractorAuthorString,
                        approverString,
                        state
                    });
                }
            }

            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, optionsListWorksheet, firstWorksheet);

            // Adjust Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { "WBS", allWbs.Count },
                { "CLINs", allClins.Count },
                { "Authors", allAuthors.Count },
                { "Approvers", allApprovers.Count },
                { "Status", allStatus.Count },
                { "Material", allMaterials.Count},
                { "Subcontractor_Authors", allSubcontratorAuthors.Count },
                { "MultiBOE", allMultiBOE.Count }
            };

            ExcelExporter.AdjustDefinedNames(toReturn, lengths);

            return toReturn;
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Breaks down AD Groups into individual users
        /// </summary>
        /// <param name="incomingUsers">incoming users potentially containing AD groups</param>
        /// <returns>individual users</returns>
        private Collection<UserDTO> CleanupAdGroupsFromUsers(Collection<UserDTO> incomingUsers)
        {
            List<UserDTO> tempUsers = new List<UserDTO>();

            // all users can contain groups. need to break those down.
            foreach (UserDTO user in incomingUsers)
            {
                if (user.NTID.Contains('.')) // AD group name
                {
					ICollection<UserData> members = this.adUtils.GetAdGroupUsers(user.DisplayName);

                    ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
                    
                    List<int> userIds = new List<int>();

                    foreach (UserData member in orderedMembers)
                    {
                        int userId;
                        bool userExists = this._IUserDTODataLoader.UserExists(member.Ntid, out userId);

                        if (userExists)
                        {
                            userIds.Add(userId);
                        }
                    }

                    tempUsers.AddRange(this._IUserDTODataLoader.GetByIds(userIds));
                }
                else // just a regular user
                {
                    tempUsers.Add(user);
                }
            }

            return new Collection<UserDTO>(tempUsers);
        }

        /// <summary>
        /// Adds another blank task type to the current task for the BOE
        /// </summary>
        /// <param name="taskContainer">The task container.</param>
        /// <returns>
        /// A new Table Row for a TaskTypeRow if the taskContainer contains one.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">taskContainer</exception>
        private TableRow AddAnotherTaskType(BOEExportTaskContainer taskContainer)
        {
            if (taskContainer == null)
            {
                throw new ArgumentNullException(nameof(taskContainer));
            }

            TableRow toReturn = null;

            if (taskContainer.TaskTypeRow != null)
            {
                toReturn = taskContainer.TaskTypeRow;
                taskContainer.TaskTypeRow = toReturn.CloneNode(true) as TableRow;
                toReturn.InsertAfterSelf(taskContainer.TaskTypeRow);
            }

            return toReturn;
        }

        /// <summary>
        /// Populates the non-task-specific items in the template with BOE data.
        /// </summary>
        /// <param name="document">The Word document to populate</param>
        /// <param name="boeExportModelView">Object to hold most of the BOE's data</param>
        private void PopulateGeneralContent(WordprocessingDocument document, BOEExportModelView boeExportModelView)
        {
			// Get all tagged elements in Header
			IEnumerable<SdtAlias> headerElements = from footerPart in document.MainDocumentPart.HeaderParts
                from sdtElement in footerPart.Header.Descendants<SdtAlias>()
                select sdtElement;

			// Get all tagged elements in Main Document
			IEnumerable<SdtAlias> mainDocumentElements = document.MainDocumentPart.Document.Descendants<SdtAlias>();

			// Get all tagged elements in Footer
			IEnumerable<SdtAlias> footerElements = from footerPart in document.MainDocumentPart.FooterParts
                from sdtElement in footerPart.Footer.Descendants<SdtAlias>()
                select sdtElement;

            // Iterate over all SdtElements in the document
            foreach (SdtAlias alias in headerElements.Concat(mainDocumentElements).Concat(footerElements).ToList())
            {
                // Get the title of this Alias
                string sdtTitle = alias.Val.Value;

				// Get the Element that encapsulates the current alias
				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();

                // If the current element is not null, populate it with the appropriate data from the BOEExportModelView
                if (element != null)
                {
                    if (sdtTitle == FieldName_HeaderFooter)
                    {
                        SetElementText(element,
                            (boeExportModelView.ContainsOCI) ?
                                "Organizational Conflict of Interest - Lockheed Martin Proprietary Information" :
                                "Lockheed Martin Proprietary Information");
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_PrintDate)
                    {
                        SetElementText(element, DateTime.Today.ToShortDateString());
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_ProgramName)
                    {
                        SetElementText(element, boeExportModelView.ProgramName);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_WSDescription)
                    {
                        SetElementText(element, boeExportModelView.WorkspaceDescription);
                        alias.RemoveIt();
                    }
                }
            }
        }

        /// <summary>
        /// Populates the non-task-specific items in the template with BOE data.
        /// </summary>
        /// <param name="boeContainer">The Word document to populate</param>
        /// <param name="boeExportModelView">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelView">Object to hold data for the BOE Summary Grid</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="mainPart">Main document part</param>
        /// <param name="templateType">Template type</param>
        /// <param name="counters">The counters.</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateBOEContent(OpenXmlElement boeContainer, BOEExportModelView boeExportModelView, List<BOESummaryGridModelView> boeSummaryGridModelView,
            BOEExportInputs exportInputs, MainDocumentPart mainPart, ExcelReportTemplateType templateType, ref ChunkCounter counters)
        {
            // Get all tagged elements in Main Document
            HashSet<SdtAlias> boeElements = new HashSet<SdtAlias>(boeContainer.Descendants<SdtAlias>().ToList());
            string useFontSize = this.GetFontSize(24, boeExportModelView.ExportFormat.TemplateType);
            string useHeaderFontSize = this.GetHeaderFontSize(24, boeExportModelView.ExportFormat.TemplateType);
            string useFont = this.GetFont("Times New Roman", boeExportModelView.ExportFormat.TemplateType);

            SdtAlias SummaryAlias1 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryElementofCost, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias1 != null)
            {
                this.PopulateBOEContent_SummaryElementofCost(boeSummaryGridModelView, boeElements, SummaryAlias1);
            }

            if (boeExportModelView.IsMaterial)
            {
                SdtAlias SummaryAlias4 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryHourByDateContainer, StringComparison.CurrentCultureIgnoreCase));
                if (SummaryAlias4 != null)
                {
                    PopulateBOEContent_MaterialSummaryHourByDateContainer(SummaryAlias4);
                }
            }
            else
            {
                // hash set of possible tags for the Summary Hour By Date Table
                HashSet<string> SummaryHourByDateTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { FieldName_SummaryHourByDate, FieldName_MSTSummaryHourByDate,
                    FieldName_MSTSummaryHourByDate_TopTotal, FieldName_MSTSummaryHourByDate_NoShading_TopTotal, FieldName_GSMOSummaryHourByDate
                };

                // find if table is used and which tag it uses
                SdtAlias SummaryAlias5 = boeElements.LastOrDefault(s => SummaryHourByDateTags.Contains(s.Val.Value));

                if (SummaryAlias5 != null)
                {
                    this.PopulateBOEContent_SummaryHourByDate(boeExportModelView, exportInputs, templateType, boeElements, useFontSize, useHeaderFontSize, useFont, SummaryAlias5);
                }
            }

            // hash set of possible tags for the Summary Cost By Date Table
            HashSet<string> SummaryCostByDateTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { FieldName_SummaryCostByDate, FieldName_MSTSummaryCostByDate,
                FieldName_MSTSummaryCostByDate_TopTotal, FieldName_MSTSummaryCostByDate_NoShading_TopTotal };

            // find if table is used and which tag it uses
            SdtAlias SummaryAlias6 = boeElements.LastOrDefault(s => SummaryCostByDateTags.Contains(s.Val.Value));
            if (SummaryAlias6 != null)
            {
                this.PopulateBOEContent_SummaryCostByDate(boeExportModelView, exportInputs, boeElements, useFontSize, useHeaderFontSize, useFont, SummaryAlias6);
            }

            // If the template contains the Summary Hour by Resource table in the BOE Header, populate it with all summarized resource types, except travel
            SdtAlias SummaryAlias8 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryHourByResource, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias8 != null)
            {
                this.PopulateBOEContent_SummaryHourByResource(boeExportModelView, boeElements, SummaryAlias8);
            }

            // Populate the BOE-level custom fields if there are any
            SdtAlias SummaryAlias9 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_BOECustomFields, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias9 != null)
            {
                this.PopulateBOEContent_BOECustomFields(boeExportModelView, exportInputs, SummaryAlias9);
            }

            // Populate BOE Summary Table of Hours for SSDS Template
            SdtAlias SummaryAlias10 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryTableOfHours, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias10 != null)
            {
                this.PopulateBOEContent_BOESummaryTableOfHours(boeExportModelView, exportInputs, SummaryAlias10);
            }

            SetFieldWithValuePlainText(boeElements, FieldName_ProgramName, boeExportModelView.ProgramName);
            SetFieldWithValuePlainText(boeElements, FieldName_SolicitationNumber, boeExportModelView.SolicitationNumber);
            SetFieldWithValuePlainText(boeElements, FieldName_BOETitle, boeExportModelView.BOETitle);
            SetFieldWithValuePlainText(boeElements, FieldName_GenBOEBOEID, boeExportModelView.BoeID.ToString());
            SetFieldWithValuePlainText(boeElements, FieldName_TrackingNumber, boeExportModelView.TrackingNumber);
            SetFieldWithValuePlainText(boeElements, FieldName_ProductLine, string.Empty);

            SetFieldWithValuePlainText(boeElements, FieldName_BOEPWS, boeExportModelView.ExportFields.ContainsKey(FieldName_BOEPWS) ? boeExportModelView.ExportFields[FieldName_BOEPWS] : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_IPT, boeExportModelView.ExportFields.ContainsKey(FieldName_IPT) ? boeExportModelView.ExportFields[FieldName_IPT] : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_Asset, boeExportModelView.ExportFields.ContainsKey(FieldName_Asset) ? boeExportModelView.ExportFields[FieldName_Asset] : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_ProposalSubmittalDate, boeExportModelView.ProposalSubmittalDate.HasValue && !boeExportModelView.ProposalSubmittalDate.Equals(DateTime.MinValue) ? boeExportModelView.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy") : string.Empty);

            SetFieldWithValuePlainText(boeElements, FieldName_ContractStartDate, boeExportModelView.ContractStartDate.Value.ToString("MM/yyyy"));
            SetFieldWithValuePlainText(boeElements, FieldName_ContractEndDate, boeExportModelView.ContractEndDate.Value.ToString("MM/yyyy"));
            SetFieldWithValuePlainText(boeElements, FieldName_WBSTitle, boeExportModelView.WBSTitle);
            SetFieldWithValuePlainText(boeElements, FieldName_StartDate, !boeExportModelView.StartDate.Equals(DateTime.MinValue) ? boeExportModelView.StartDate.ToString("MM/yyyy") : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_EndDate, !boeExportModelView.EndDate.Equals(DateTime.MinValue) ? boeExportModelView.EndDate.ToString("MM/yyyy") : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_WBSNumber, boeExportModelView.WBSNumber);
            SetFieldWithValuePlainText(boeElements, FieldName_CLIN, boeExportModelView.ExportFormat.TemplateType == ExcelReportTemplateType.DS_ES_IECS_LANDSCAPE_WITH_COST ? string.Concat(boeExportModelView.CLINNumber, " ", boeExportModelView.CLINTitle) : boeExportModelView.CLINNumber);
            SetFieldWithValuePlainText(boeElements, FieldName_CLINTitle, boeExportModelView.CLINTitle);
            SetFieldWithValuePlainText(boeElements, FieldName_CLINStartDate, boeExportModelView.CLINStartDate.HasValue ? boeExportModelView.CLINStartDate.Value.ToString("MM/yyyy") : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_CLINEndDate, boeExportModelView.CLINEndDate.HasValue ? boeExportModelView.CLINEndDate.Value.ToString("MM/yyyy") : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_BOESegregation, boeExportModelView.ExportFields.ContainsKey(FieldName_BOESegregation) ? boeExportModelView.ExportFields[FieldName_BOESegregation] : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_Date, boeExportModelView.ProposalSubmittalDate.HasValue ? boeExportModelView.ProposalSubmittalDate.Value.ToShortDateString() : string.Empty);
            SetFieldWithValuePlainText(boeElements, FieldName_PrintDate, DateTime.Today.ToShortDateString());
            SetFieldWithValuePlainText(boeElements, FieldName_PreparedBy, boeExportModelView.PreparedBy);
            SetFieldWithValuePlainText(boeElements, FieldName_PreparedByDate, boeExportModelView.SubmittedDate);
            SetFieldWithValuePlainText(boeElements, FieldName_Category, boeExportModelView.Category);
            SetFieldWithValuePlainText(boeElements, FieldName_SOWNumber, boeExportModelView.SOWNumber);
            SetFieldWithValuePlainText(boeElements, FieldName_SOWTitle, boeExportModelView.SOWTitle);
            SetFieldWithValuePlainText(boeElements, FieldName_ClassOfCost, boeExportModelView.ClassOfCost);
            SetFieldWithValuePlainText(boeElements, FieldName_AddDelete, boeExportModelView.AddDelete);
            SetFieldWithValuePlainText(boeElements, FieldName_ActivityId, boeExportModelView.ActivityId);           

            if (boeExportModelView.IsProjectMap)
            {
                SetFieldWithValuePlainText(boeElements, FieldName_BOEDescription, boeExportModelView.BOEDescription);
            }
            else
            {
                SetFieldWithValueHtmlText(mainPart, boeElements, FieldName_BOEDescription, boeExportModelView.BOEDescription, ref counters);
            }

            SetFieldWithValueHtmlText(mainPart, boeElements, FieldName_BOEDescriptionSSDS, this.ReplaceParagraphTags(boeExportModelView.BOEDescription), ref counters);

            decimal total = boeSummaryGridModelView.Sum(s => s.TotalHours).Value;
            SetFieldWithValuePlainText(boeElements, FieldName_SummaryTotal, total == 0 && !exportInputs.Workspace.IsProjectMapWorkspace ? string.Empty : Utilities.FormatStringWithPrecision(total, this.WorkspaceDecimalPrecision));

            total = boeSummaryGridModelView.Sum(s => s.TotalCost).Value;
            SetFieldWithValuePlainText(boeElements, FieldName_SummaryCostTotal, total == 0.0m && !exportInputs.Workspace.IsProjectMapWorkspace ? string.Empty : total.ToString("C0", this.CurrencyFormatter));

            total = boeExportModelView.TaskElements.Sum(t => t.taskElementLabors.Sum(l => l.Hours)).Value;
            SetFieldWithValuePlainText(boeElements, FieldName_LaborTypesTotal, total == 0 ? string.Empty : Utilities.FormatStringWithPrecision(total, this.WorkspaceDecimalPrecision));

            total = boeExportModelView.TaskElements.Sum(t => t.taskElementLabors.Sum(l => l.Cost)).Value;
            SetFieldWithValuePlainText(boeElements, FieldName_LaborCostTotal, total == 0.0m ? string.Empty : total.ToString("C0", this.CurrencyFormatter));

            SetFieldWithValuePlainText(boeElements, FieldName_SummaryLaborTypes, boeSummaryGridModelView.Select(x => x.LaborType).ToArray());
            SetFieldWithValuePlainText(boeElements, FieldName_SummaryLaborTypeHours, boeSummaryGridModelView.Where(s => s.TotalHours.HasValue)
                .Select(s => s.TotalHours.Value == 0 ? string.Empty : Utilities.FormatStringWithPrecision(s.TotalHours.Value, this.WorkspaceDecimalPrecision)).ToArray());
            SetFieldWithValuePlainText(boeElements, FieldName_SummaryLaborTypeCost, boeSummaryGridModelView.Where(s => s.TotalCost.HasValue)
                .Select(s => s.TotalCost.Value == 0.0m ? string.Empty : s.TotalCost.Value.ToString("C0", this.CurrencyFormatter)).ToArray());
            SetFieldWithValuePlainText(boeElements, FieldName_ApprovedBy, boeExportModelView.Approvers.Select(a => a.ApprovedBy).ToArray());
            SetFieldWithValuePlainText(boeElements, FieldName_ApprovedByDate, boeExportModelView.Approvers.Select(a => a.ApprovedDate).ToArray());

            #region Custom Areas

            SdtAlias alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_ResourceSummaryByResourceIDTable);
            if (alias != null)
            {
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    this.PopulateResourceSummaryByResourceIDTable(element, boeExportModelView);
                    alias.RemoveIt();
                }
            }

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_ResourceSummaryByResourceIDLaborCategoryLocationTable);
            if (alias != null)
            {
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    this.PopulateResourceSummaryByResourceIDLaborCategoryLocationTable(element, boeExportModelView, exportInputs);
                    alias.RemoveIt();
                }
            }

            #region SOW Custom Field for Underseas Template (USS)

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_CustomField_SowId);
            if (alias != null)
            {
                KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customField = exportInputs
                    .AssignedBoeIdsAndCustomFieldValuesMapping.Where(x => x.Key == boeExportModelView.BoeID)
                    .Select(x => x.Value).FirstOrDefault()
                    .FirstOrDefault(x => x.Value.CustomFieldName == SOW_CUSTOM_FIELD);

                CustomFieldValueDTO fieldValue = customField.Key;

                string value = fieldValue == null ? string.Empty : fieldValue.CustomFieldValueName;

                SetFieldWithValuePlainText(boeElements, FieldName_CustomField_SowId, value);

                alias.RemoveIt();
            }

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_CustomField_SowDesc);
            if (alias != null)
            {
                KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customField = exportInputs
                    .AssignedBoeIdsAndCustomFieldValuesMapping.Where(x => x.Key == boeExportModelView.BoeID)
                    .Select(x => x.Value).FirstOrDefault()
                    .FirstOrDefault(x => x.Value.CustomFieldName == SOW_CUSTOM_FIELD);

                CustomFieldValueDTO fieldValue = customField.Key;

                string value = fieldValue == null ? string.Empty : fieldValue.CustomFieldValueDescription;

                SetFieldWithValuePlainText(boeElements, FieldName_CustomField_SowDesc, value);

                alias.RemoveIt();
            }

            #endregion

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_CustomField_RightsInData);
            if (alias != null)
            {
                IDictionary<CustomFieldValueDTO, CustomFieldDTO> customFieldDictionary = exportInputs
                    .AssignedBoeIdsAndCustomFieldValuesMapping.Where(x => x.Key == boeExportModelView.BoeID)
                    .Select(x => x.Value).FirstOrDefault();

                KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customField = customFieldDictionary != null
                    ? customFieldDictionary.FirstOrDefault(x => x.Value.CustomFieldName.ToLower() == RIGHTS_IN_DATA_CUSTOM_FIELD.ToLower())
                    : new KeyValuePair<CustomFieldValueDTO, CustomFieldDTO>();

                CustomFieldValueDTO fieldValue = customField.Key;

                string value = fieldValue == null ? string.Empty : fieldValue.CustomFieldValueDescription;

                SetFieldWithValuePlainText(boeElements, FieldName_CustomField_RightsInData, value);

                alias.RemoveIt();
            }

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_ProjectMapResourceSummaryTable);
            if (alias != null)
            {
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    this.ProcessProjectMapResourceSummaryTable(element, boeExportModelView);
                    alias.RemoveIt();
                }
            }

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_SourcesOfData);
            if (alias != null)
            {
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    // Gets rid of the "Of Data" that has been a repeated problem
                    // For some reason a seperate run is created for each word (Sources of Data) in the template
                    IList<Run> runs = element.Descendants<Run>().ToList();
                    for (int i = runs.Count - 1; i > 0; i--)
                    {
                        runs[i].RemoveIt();
                    }

                    SdtContentBlock item = element.ChildElements.OfType<SdtContentBlock>().FirstOrDefault();
                    if (item != null)
                    {
                        string SourcesOfData = boeExportModelView.DataSource;
                        if (exportInputs.Workspace.IsProjectMapWorkspace)
                        {
                            WordUtilities.SetElementText(item, SourcesOfData);
                        }
                        else
                        {
                            WordUtilities.SetElementTextWithHTML(mainPart, item, SourcesOfData, ref counters);
                        }

                        // if the template has this field in a table row and it is removed, the document will be unable to open
                        // so the extra space needs to be kept if the Sources of Data field is empty
                        bool keepEmptyTableRow = (item.Ancestors<TableRow>().FirstOrDefault() != null) && string.IsNullOrEmpty(SourcesOfData);

                        // removes blank spaces in output and "Sources" placeholder
                        foreach (OpenXmlElement child in item.ChildElements)
                        {
                            if ((string.IsNullOrEmpty(child.InnerText) || (child.InnerText == "Sources" && child.HasAttributes)) && !keepEmptyTableRow)
                            {
                                if (!child.Descendants<Drawing>().Any())  // Bug 32436 - Avoid deleting images
                                {
                                    child.Remove();
                                }
                            }
                        }
                    }

                    alias.RemoveIt();
                }
            }

            alias = boeElements.LastOrDefault(x => x.Val.Value == FieldName_BoeSummaryTable);
            if (alias != null)
            {
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    this.PopulateBOELevelSummaryTable(element, boeExportModelView, exportInputs);
                    alias.RemoveIt();
                }
            }

            #endregion
        }

        #region Helper methods for setting word elements/fields w/ their values

        /// <summary>
        /// Sets field values with plain text
        /// </summary>
        /// <param name="boeElements">Boe Elements</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="fieldValues">Field Values</param>
        private static void SetFieldWithValuePlainText(HashSet<SdtAlias> boeElements, string fieldName, string[] fieldValues)
        {
			SdtAlias alias = boeElements.LastOrDefault(x => x.Val.Value == fieldName);
            if (alias != null)
            {
				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    SetElementText(element, fieldValues);
                    alias.RemoveIt();
                }
            }
        }

        /// <summary>
        /// Sets field values with plain text
        /// </summary>
        /// <param name="boeElements">Boe Elements</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="fieldValue">Field Values</param>
        private static void SetFieldWithValuePlainText(HashSet<SdtAlias> boeElements, string fieldName, string fieldValue)
        {
            // this allows us to deal with multiple instances of the same field needing to be filled out w/ data
            boeElements.Where(x => x.Val.Value == fieldName).ToList().ForEach(alias =>
                {
					SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                    if (element != null)
                    {
                        SetElementText(element, fieldValue);
                        alias.RemoveIt();
                    }
                });
        }

        /// <summary>
        /// Set Field values with Html text
        /// </summary>
        /// <param name="mainPart">Main Part of the document</param>
        /// <param name="boeElements">Task Elements</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="fieldValue">Field Values</param>
        /// <param name="counters">Counters for alt-chunks</param>
        private static void SetFieldWithValueHtmlText(MainDocumentPart mainPart, HashSet<SdtAlias> boeElements, string fieldName, string fieldValue, ref ChunkCounter counters)
        {
			SdtAlias alias = boeElements.LastOrDefault(x => x.Val.Value == fieldName);
            if (alias != null)
            {
				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    WordUtilities.SetElementTextWithHTML(mainPart, element, fieldValue, ref counters);
                    alias.RemoveIt();
                }
            }
        }

        #endregion

        #region PopulateBOEContent_SummaryAliasX Methods

        /// <summary>
        /// Populates the Summary Element of Cost
        /// </summary>
        /// <param name="boeSummaryGridModelView">Modelview for the BOE Summary grid</param>
        /// <param name="boeElements">Template elements within the BOE Container</param>
        /// <param name="SummaryAlias">SdtAlias for SummaryElementofCost</param>
        private void PopulateBOEContent_SummaryElementofCost(List<BOESummaryGridModelView> boeSummaryGridModelView, HashSet<SdtAlias> boeElements, SdtAlias SummaryAlias)
        {
			SdtElement element = SummaryAlias.Ancestors<SdtElement>().FirstOrDefault();
			IEnumerable<string> ElementofCostDescriptons = from s in boeSummaryGridModelView
                select s.Category.ToDescription();

            SetElementText(element, (ElementofCostDescriptons.Distinct().OrderBy(x => x).ToArray()));
            SummaryAlias.RemoveIt();

            var Groups = boeSummaryGridModelView.Where(s => s.TotalCost.HasValue).GroupBy(x => x.Category)
                .Select(g => new { EOC = g.Key, CostSum = g.Sum(x => x.TotalCost), HourSum = g.Sum(x => x.TotalHours) }).OrderBy(x => x.EOC.ToDescription());

            SdtAlias SummaryAlias2 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryElementofCostHours, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias2 != null)
            {
                element = SummaryAlias2.Ancestors<SdtElement>().FirstOrDefault();
                SetElementText(element, (Groups.Select(x => Utilities.FormatStringWithPrecision(x.HourSum.Value, this.WorkspaceDecimalPrecision))).ToArray());
                SummaryAlias2.RemoveIt();
            }

            SdtAlias SummaryAlias3 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryElementofCostCost, StringComparison.CurrentCultureIgnoreCase));
            if (SummaryAlias3 != null)
            {
                element = SummaryAlias3.Ancestors<SdtElement>().FirstOrDefault();
                SetElementText(element, (Groups.Select(x => x.CostSum.Value.ToString("C0", this.CurrencyFormatter))).ToArray());
                SummaryAlias3.RemoveIt();
            }
        }

        /// <summary>
        /// Removes SummaryHourByDateContainer for Material
        /// </summary>
        /// <param name="SummaryAlias4">SdtAlias for the SummaryHourByDateContainer</param>
        private static void PopulateBOEContent_MaterialSummaryHourByDateContainer(SdtAlias SummaryAlias4)
        {
			SdtElement element = SummaryAlias4.Ancestors<SdtElement>().FirstOrDefault();
            element.RemoveAllChildren();
            SummaryAlias4.RemoveIt();
        }

        /// <summary>
        /// Populates the SummaryHourByDate table
        /// </summary>
        /// <param name="boeExportModelView">Modelview of the BOE export</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="templateType">template being used</param>
        /// <param name="boeElements">Template elements within the BOEContainer</param>
        /// <param name="useFontSize">Font size for non-header cells</param>
        /// <param name="useHeaderFontSize">Font size for header cells</param>
        /// <param name="useFont">Font to use in the table</param>
        /// <param name="SummaryAlias5">sdtAlias of the table</param>
        private void PopulateBOEContent_SummaryHourByDate(BOEExportModelView boeExportModelView, BOEExportInputs exportInputs, ExcelReportTemplateType templateType, HashSet<SdtAlias> boeElements, string useFontSize, string useHeaderFontSize, string useFont, SdtAlias SummaryAlias5)
        {
			SdtElement element = SummaryAlias5.Ancestors<SdtElement>().FirstOrDefault();
            ICollection<BoeTaskElementDTO> taskElementCollection = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
            if (taskElementCollection.Any())
            {
                if (templateType == ExcelReportTemplateType.LMSI_STANDARD_LANDSCAPE)
                {
                    List<LaborRollup> rollupDataByResource = this.GetLaborHoursRollupByResource(boeExportModelView.TaskElements);
                    this.PopulateLaborHoursSummaryByResource(element, rollupDataByResource);
                }
                else
                {
                    Collection<LaborRollupByDate> RollupByDate = this.GetRollupByYearData(taskElementCollection).ToCollection();

                    JustificationValues numberAlignment = JustificationValues.Center;
                    string fontSize = useFontSize;
                    string headerFontSize = useHeaderFontSize;
                    if (SummaryAlias5.Val.Value == FieldName_GSMOSummaryHourByDate)
                    {
                        numberAlignment = JustificationValues.Right;
                        fontSize = headerFontSize = "20";
                    }

                    this.PopulateBoeYearSummaryRollup(element, RollupByDate, this.DefaultHoursFormat, null, useFont, fontSize, headerFontSize, SummaryAlias5.Val.Value, numberAlignment);
                }
            }
            else
            {
                SdtAlias SummaryAlias5a = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryHourByDateContainer, StringComparison.CurrentCultureIgnoreCase));
                if (SummaryAlias5a != null)
                {
                    element = SummaryAlias5a.Ancestors<SdtElement>().FirstOrDefault();
                    element.RemoveAllChildren();
                }

                SummaryAlias5a = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_BOESpreadTotalsTitle, StringComparison.CurrentCultureIgnoreCase));
                if (SummaryAlias5a != null)
                {
                    element = SummaryAlias5a.Ancestors<SdtElement>().FirstOrDefault();
                    element.RemoveAllChildren();
                }
            }

            SummaryAlias5.RemoveIt();
        }

        /// <summary>
        /// Populates the SummaryCostByDate table
        /// </summary>
        /// <param name="boeExportModelView">Modelview of the BOE export</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeElements">Template elements within the BOEContainer</param>
        /// <param name="useFontSize">Font size for non-header cells</param>
        /// <param name="useHeaderFontSize">Font size for header cells</param>
        /// <param name="useFont">Font to use in the table</param>
        /// <param name="SummaryAlias6">sdtAlias of the table</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateBOEContent_SummaryCostByDate(BOEExportModelView boeExportModelView, BOEExportInputs exportInputs, HashSet<SdtAlias> boeElements, string useFontSize, string useHeaderFontSize, string useFont, SdtAlias SummaryAlias6)
        {
			SdtElement element = SummaryAlias6.Ancestors<SdtElement>().FirstOrDefault();
            NumberFormatInfo currencyFormatter = new NumberFormatInfo();
            currencyFormatter.CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR;
            currencyFormatter.CurrencySymbol = string.Empty;
            currencyFormatter.CurrencyDecimalDigits = 2;
            Collection<LaborRollupByDate> Rollup = null;

            if (boeExportModelView.IsMaterial)
            {
                Rollup = new Collection<LaborRollupByDate>();
            }
            else
            {
                Rollup = this.GetCostRollup(exportInputs, boeExportModelView);
            }

            if (Rollup != null && Rollup.Any())
            {
                string format = "C2";
                if (SummaryAlias6.Val.Value.Contains("MST"))
                {
                    format = BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS;
                }

                this.PopulateBoeYearSummaryRollup(element, Rollup, format, this.CurrencyFormatter, useFont, useFontSize, useHeaderFontSize, SummaryAlias6.Val.Value, JustificationValues.Center);

                SummaryAlias6.RemoveIt();
            }
            else if (boeExportModelView.ExportFormat.TemplateType != ExcelReportTemplateType.LMSI_NISSC_LANDSCAPE_WITH_NON_LABOR_COST && !SummaryAlias6.Val.Value.Contains("MST"))
            {
                element.RemoveAllChildren();
                Table table = this.CreateRollupTable();
                List<string> Headers = new List<string>() { "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };
                table.Append(this.CreateRollupHeaderRow(Headers, useFont, useHeaderFontSize, false));
                table.Append(this.CreateTableBlankRow(Headers.Count));
                element.Append(table);
                SummaryAlias6.RemoveIt();
            }
            else // remove table and title for LMSI-NISSC template if empty
            {
                SummaryAlias6.RemoveIt();
				SdtAlias SummaryAlias7 = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_SummaryCostByDateContainer, StringComparison.CurrentCultureIgnoreCase));
                if (SummaryAlias7 != null)
                {
					SdtElement element2 = SummaryAlias7.Ancestors<SdtElement>().FirstOrDefault();
                    element2.RemoveIt();
                    SummaryAlias7.RemoveIt();
                }
            }
        }

        /// <summary>
        /// Populates the SummaryHourByResource table
        /// </summary>
        /// <param name="boeExportModelView">Modelview of the BOE export</param>
        /// <param name="boeElements">Template elements within the BOEContainer</param>
        /// <param name="SummaryAlias8">sdtAlias of the table</param>
        private void PopulateBOEContent_SummaryHourByResource(BOEExportModelView boeExportModelView, HashSet<SdtAlias> boeElements, SdtAlias SummaryAlias8)
        {
			SdtElement element = SummaryAlias8.Ancestors<SdtElement>().FirstOrDefault();
            List<BOEExportTaskElementLabor> allExportResourceTypes = new List<BOEExportTaskElementLabor>();

            // Only populate these if it's not material, otherwise it would not be used
            if (!boeExportModelView.IsMaterial)
            {
                allExportResourceTypes = boeExportModelView.TaskElements
                    .SelectMany(x => x.taskElementLabors)
                    .Where(x => x.ExportFields.ContainsKey(FieldName_TaskTypeDescription) && x.ElementType != BOEExportTaskElementType.Travel).ToList();
            }

            if (allExportResourceTypes.Any())
            {
                // Summarize all resources for the BOE and group by Resource Name and Description
                List<LaborResourceRollup> rollupDataByResource = this.GetLaborResourceHoursRollup(allExportResourceTypes);
                this.PopulateLaborResourcesHoursSummary(element, rollupDataByResource);
            }
            else
            {
                // Delete Resource Summary container if no resources exist to populate it with
                SdtAlias SummaryAlias8a = boeElements.LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceSummaryContainer, StringComparison.CurrentCultureIgnoreCase));

                if (SummaryAlias8a != null)
                {
                    element = SummaryAlias8a.Ancestors<SdtElement>().FirstOrDefault();
                    if (element != null)
                    {
                        element.Remove();
                    }
                }
            }
        }

        /// <summary>
        /// Populates the BOE-level custom fields
        /// </summary>
        /// <param name="boeExportModelView">Modelview of the BOE export</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="SummaryAlias9">sdtAlias of the custom field template element</param>
        private void PopulateBOEContent_BOECustomFields(BOEExportModelView boeExportModelView, BOEExportInputs exportInputs, SdtAlias SummaryAlias9)
        {
            SdtElement boeCustomFieldsElement = SummaryAlias9.Ancestors<SdtElement>().FirstOrDefault();
            if (boeCustomFieldsElement != null)
            {
                IDictionary<CustomFieldValueDTO, CustomFieldDTO> boeCustomFields = exportInputs.AssignedBoeIdsAndCustomFieldValuesMapping.Where(x => x.Key == boeExportModelView.BoeID).Select(x => x.Value).FirstOrDefault();

                if (boeCustomFields != null && boeCustomFields.Any())
                {
                    IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> cfValues = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();
                    cfValues.Add(0, boeCustomFields);
                    this.PopulateCustomFields(boeCustomFieldsElement, cfValues);
                }
                else
                {
                    boeCustomFieldsElement.RemoveIt();
                }
            }
            else
            {
                SummaryAlias9.RemoveIt();
            }
        }

        /// <summary>
        /// Populate the BOE Summary Table of Hours (SSDS Template)
        /// </summary>
        /// <param name="boeExportModelView">export model view</param>
        /// <param name="exportInputs">export inputs</param>
        /// <param name="SummaryAlias10">alias for the table</param>
        private void PopulateBOEContent_BOESummaryTableOfHours(BOEExportModelView boeExportModelView, BOEExportInputs exportInputs, SdtAlias SummaryAlias10)
        {
            SdtElement boeSummaryTableOfHoursElement = SummaryAlias10.Ancestors<SdtElement>().FirstOrDefault();
            if (boeSummaryTableOfHoursElement != null)
            {
                // Get resource modelviews grouped by Govt Labor Category (Function) then by Key Personnel (Field-B)
                List<BOEExportTaskElementLabor> allExportResourceTypes =
                    boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToList();

                // Get resouce data from exportInputs
                List<ResourceTypeDto> resourceInputs = exportInputs.TaskElements.SelectMany(x => x.taskElementLabors)
                    .Where(y => allExportResourceTypes.Select(z => z.ExportFields[FieldName_LaborTypeID])
                        .Contains(y.Id.ToString())).ToList();
                
                // Set up header row
                Table table = boeSummaryTableOfHoursElement.Descendants<Table>().First();
                TableRow headerRow = table.Descendants<TableRow>().First();

                // Append Year Headers
                int startYear = boeExportModelView.StartDate.Year;
                int endYear = boeExportModelView.EndDate.Year;

                WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(headerRow, FieldName_YearLabel),
                    "CY " + startYear.ToString());

                for (int year = startYear + 1; year <= endYear; year++)
                {
                    this.AppendCellToRow(headerRow, "CY " + year.ToString());
                }

                // Append Total Column Header
                this.AppendCellToRow(headerRow, "Total");

                // Set header row property
                if (headerRow.TableRowProperties == null)
                {
                    headerRow.TableRowProperties = new TableRowProperties();
                }

                headerRow.TableRowProperties.AppendChild(new TableHeader());

                // Populate data rows
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(boeSummaryTableOfHoursElement, BOEExporterConstants.Marker_DataRow);
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                if (templateDataRow != null)
                {
                    this.SetCantSplit(templateDataRow);
                    TableRow currentInsertionRow = templateDataRow;
                    foreach(BOEExportTaskElementLabor resource in allExportResourceTypes)
                    {
                        // create a new row in the table
                        TableRow row = this.CloneMarkedTemplateRow(templateDataRow);

                        // get dto for resource type
                        ResourceTypeDto resourceInput =
                            resourceInputs.FirstOrDefault(x => x.Id.ToString() == resource.ExportFields[FieldName_LaborTypeID]);
                        if (resourceInput == null)
                        {
                            continue;
                        }

                        // get perf org dto used by resource type
                        PerformingOrgDTO perfOrg =
                            exportInputs.PerformingOrgsUsedInBoes.FirstOrDefault(
                                x => x.Id == resourceInput.PerformingOrgID);
                        if (perfOrg == null)
                        {
                            continue;
                        }

                        // populate the row
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(row, FieldName_GovtLaborCategory),
                            resource.ExportFields[FieldName_GovtLaborCategory]);
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(row, FieldName_ResourceName),
                            resource.ExportFields[FieldName_ResourceName]);
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(row, FieldName_PerfOrg),
                            perfOrg.PerformingOrgDesc);
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(row, FieldName_KeyPersonnel),
                            resource.ExportFields[FieldName_KeyPersonnel]);

                        // Populate first year
                        ICollection<ResourceSpreadDto> spreadsForFirstYear = resourceInput.LaborSpreads.Where(x => x.LaborSpreadDate.Year == startYear).ToCollection();
                        decimal firstYearValue = spreadsForFirstYear.Sum(x => x.LaborSpreadValue);
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(row, FieldName_YearValue),
                            Utilities.FormatStringWithPrecision(firstYearValue, this.WorkspaceDecimalPrecision));

                        // populate remaining year values
                        for (int year = startYear + 1; year <= endYear; year++)
                        {
                            ICollection<ResourceSpreadDto> spreadsForYear = resourceInput.LaborSpreads.Where(x => x.LaborSpreadDate.Year == year).ToCollection();
                            decimal value = spreadsForYear.Sum(x => x.LaborSpreadValue);
                            this.AppendCellToRow(row, Utilities.FormatStringWithPrecision(value, this.WorkspaceDecimalPrecision));
                        }

                        // populate total
                        this.AppendCellToRow(row, Utilities.FormatStringWithPrecision(resourceInput.ValueSpread ?? 0m, this.WorkspaceDecimalPrecision) );

                        // add the row to the table
                        currentInsertionRow.InsertAfterSelf(row);
                        currentInsertionRow = row;
                    }
                }

                // remove template row
                templateDataRow.Remove();

                // Populate totals row
                SdtElement totalsRowMarkerTag = WordUtilities.GetTaggedChildElement(boeSummaryTableOfHoursElement, BOEExporterConstants.Marker_TotalsRow);
                TableRow totalsRow = totalsRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                if (totalsRow != null)
                {
                    this.SetCantSplit(totalsRow);

                    // Populate first year totals
                    ICollection<ResourceSpreadDto> allSpreadsForFirstYear = resourceInputs.SelectMany(x => x.LaborSpreads).Where(x => x.LaborSpreadDate.Year == startYear).ToCollection();
                    decimal firstYearTotal = allSpreadsForFirstYear.Sum(x => x.LaborSpreadValue);
                    WordUtilities.SetElementText(
                        WordUtilities.GetTaggedChildElement(totalsRow, FieldName_YearValueTotal),
                        Utilities.FormatStringWithPrecision(firstYearTotal, this.WorkspaceDecimalPrecision));

                    // Populate remaining year totals
                    for (int year = startYear + 1; year <= endYear; year++)
                    {
                        ICollection<ResourceSpreadDto> allSpreadsForYear = resourceInputs.SelectMany(x => x.LaborSpreads).Where(x => x.LaborSpreadDate.Year == year).ToCollection();
                        decimal yearTotal = allSpreadsForYear.Sum(x => x.LaborSpreadValue);
                        this.AppendCellToRow(totalsRow,
                            Utilities.FormatStringWithPrecision(yearTotal, this.WorkspaceDecimalPrecision));
                    }

                    // Populate overall total
                    decimal overallTotal = resourceInputs.Sum(x => x.ValueSpread ?? 0m);
                    this.AppendCellToRow(totalsRow,
                        Utilities.FormatStringWithPrecision(overallTotal, this.WorkspaceDecimalPrecision));
                }
            }
        }

        #endregion

        /// <summary>
        /// Create table for rollup
        /// </summary>
        /// <returns>returns rollup table</returns>
        protected virtual Table CreateRollupTable()
        {
            Table table = new Table();
            TableProperties props = new TableProperties(
                new TableBorders(
                    new TopBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new BottomBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new LeftBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new RightBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideHorizontalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideVerticalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    }), new TableWidth() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Pct), Width = "4995" },
                new TableLook() { Val = "04A0", FirstRow = true, LastRow = false, FirstColumn = true, LastColumn = false, NoHorizontalBand = false, NoVerticalBand = true },
                new TableCellMarginDefault()
                {
                    TableCellLeftMargin = new TableCellLeftMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Nil), Width = 0 },
                    TableCellRightMargin = new TableCellRightMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Nil), Width = 0 },
                    TopMargin = new TopMargin() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa), Width = "144" }
                });
            table.AppendChild<TableProperties>(props);

            return table;
        }

        /// <summary>
        /// Create table for GSMO rollup
        /// </summary>
        /// <param name="layout">Table layout value - fixed or auto</param>
        /// <returns>returns GSMO rollup table</returns>
        protected virtual Table CreateGSMORollupTable(TableLayoutValues layout)
        {
            Table table = new Table();
            TableProperties props = new TableProperties(
                new TableBorders(
                    new TopBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new BottomBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new LeftBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new RightBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideHorizontalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideVerticalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    }),
                new TableLook() { Val = "04A0", FirstRow = true, LastRow = false, FirstColumn = true, LastColumn = false, NoHorizontalBand = false, NoVerticalBand = true },
                new TableCellMarginDefault()
                {
                    TableCellLeftMargin = new TableCellLeftMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Dxa), Width = 58 },
                    TableCellRightMargin = new TableCellRightMargin() { Type = new EnumValue<TableWidthValues>(TableWidthValues.Dxa), Width = 58 },
                    TopMargin = new TopMargin() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa), Width = "58" },
                    BottomMargin = new BottomMargin() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa), Width = "58" }
                },
                new TableJustification() { Val = TableRowAlignmentValues.Center },
                new TableLayout() { Type = layout }, new TableWidth() { Type = TableWidthUnitValues.Dxa, Width = "14616" }); // 14616 20ths of a point = 10.15inches 
            table.AppendChild<TableProperties>(props);

            return table;
        }

        /// <summary>
        /// Creates header row for rollup table
        /// </summary>
        /// <param name="Headers">table headers</param>
        /// <param name="inFont">font to use</param>
        /// <param name="inFontSize">font size to use</param>
        /// <param name="noAfterSpacing">bool to determine if any after spacing should be removed from text in the table</param>
        /// <returns>returns table header row</returns>
        protected virtual TableRow CreateRollupHeaderRow(List<string> Headers, string inFont, string inFontSize, bool noAfterSpacing)
        {
            TableRowProperties trp = new TableRowProperties(new TableHeader(), new CantSplit());
			TableRow tr = new TableRow();
            tr.Append(trp);
            if (Headers != null)
            {
                foreach (string header in Headers)
                {
                    JustificationValues hAlign = JustificationValues.Center;
                    TableCellProperties tcp = new TableCellProperties(
                        new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                    RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    ParagraphProperties pp;
                    if (noAfterSpacing)
                    {
                        pp = new ParagraphProperties(new Justification() { Val = hAlign }, new KeepNext() { Val = true }, new SpacingBetweenLines() { After = "0" });
                    }
                    else
                    {
                        pp = new ParagraphProperties(new Justification() { Val = hAlign }, new KeepNext() { Val = true });
                    }

                    this.PopulateTableCell(tr, header, inFont, inFontSize, tcp, pp, rp);
                }
            }
            return tr;
        }

        /// <summary>
        /// Populate the Labor Hours Summary table
        /// </summary>
        /// <param name="element">Table element</param>
        /// <param name="rollupData">Summary data</param>
        private void PopulateLaborHoursSummaryByResource(SdtElement element, List<LaborRollup> rollupData)
        {
            SdtElement tableContainerElement = element;

            if (rollupData != null && rollupData.Any())
            {
                // initialize the "insertion" row
                TableRow templateDataRow;
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, FieldName_ResourceDescription);
                if ((templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault()) != null)
                {
                    this.SetCantSplit(templateDataRow);
                    TableRow currentInsertionRow = templateDataRow;

                    foreach (LaborRollup rollupRowData in rollupData)
                    {
                        // create a new summary data row in the table
                        TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

                        // populate the row
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceDescription), rollupRowData.ResourceDescription);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_LaborTypeHours), rollupRowData.Hours.HasValue ? Utilities.FormatStringWithPrecision(rollupRowData.Hours.Value, this.WorkspaceDecimalPrecision) : "0");

                        // add the row to the table
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }

                    // can just use the existing total row (it does not need to be cloned)
                    TableRow totalsRow;
                    SdtElement dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, FieldName_TotalHours);
                    if ((totalsRow = dataTotalsRowMarkerTag.Ancestors<TableRow>().FirstOrDefault()) != null)
                    {
                        this.SetCantSplit(totalsRow);

                        // populate the overall totals
                        decimal hoursTotal = rollupData.Sum(x => x.Hours.HasValue ? x.Hours.Value : 0L);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, FieldName_TotalHours), Utilities.FormatStringWithPrecision(hoursTotal, this.WorkspaceDecimalPrecision));
                    }

                    // remove template rows
                    templateDataRow.Remove();
                }
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Populates the Summary Resource by Hour table. Sums up the hours for resources of all labor tasks in BOE
        /// and groups by Resource ID (name) and Description.
        /// </summary>
        /// <param name="tableElement">The table element.</param>
        /// <param name="rollupData">Summarized data</param>
        private void PopulateLaborResourcesHoursSummary(SdtElement tableElement, List<LaborResourceRollup> rollupData)
        {
            if (rollupData != null && rollupData.Any())
            {
                // initialize the "insertion" row
                TableRow templateDataRow;
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableElement, FieldName_ResourceDescription);
                if ((templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault()) != null)
                {
                    this.SetCantSplit(templateDataRow);
                    TableRow currentInsertionRow = templateDataRow;

                    foreach (LaborResourceRollup rollupRowData in rollupData)
                    {
                        // create a new summary data row in the table
                        TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

                        // populate the row
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceName), rollupRowData.ResourceName);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceDescription), rollupRowData.ResourceDescription);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_LaborTypeHours), rollupRowData.Hours.HasValue ? rollupRowData.Hours.Value.ToString(BOEExporterConstants.NUMERIC_FORMAT_COMMAS_NO_DECIMALS) : "0");

                        // add the row to the table
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }

                    // can just use the existing total row (it does not need to be cloned)
                    TableRow totalsRow;
                    SdtElement dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableElement, FieldName_TotalHours);
                    if ((totalsRow = dataTotalsRowMarkerTag.Ancestors<TableRow>().FirstOrDefault()) != null)
                    {
                        this.SetCantSplit(totalsRow);

                        // populate the overall totals
                        long hoursTotal = rollupData.Sum(x => x.Hours.HasValue ? x.Hours.Value : 0L);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, FieldName_TotalHours), hoursTotal.ToString(BOEExporterConstants.NUMERIC_FORMAT_COMMAS_NO_DECIMALS));
                    }

                    // remove template rows
                    templateDataRow.Remove();
                }
            }
            else
            {
                this.RemoveElement(tableElement);
            }
        }

        /// <summary>
        /// Populates the template with Custom Field Values for a BOE, tasks, or resources
        /// </summary>
        /// <param name="customFieldsElement">The sdtelement for the custom field data</param>
        /// <param name="customFieldValues">dictionaries of the custom fields and their values in a dictionary with each key representing the BOE, Task, or Resource the fields are in</param>
        private void PopulateCustomFields(SdtElement customFieldsElement, IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> customFieldValues)
        {
            if (customFieldValues.Any())
            {
                // initialize the "insertion" row
                SdtBlock contentBlockTemplate;

                SdtBlock currentInsertionBlock = contentBlockTemplate = customFieldsElement as SdtBlock;

                /*
                 * SdtBlock
                 *      SdtProperties
                 *          Tag = CustomFields-Labor
                 *      SdtContent
                 *          Paragraph
                 *              SdtRun
                 *                  SdtProperties
                 *                      Tag = CustomFieldLabel, CustomFieldID, CustomFieldDescription
                 *                  SdtContentRun
                 *                      Run
                 *                          Text
                 */

                foreach (KeyValuePair<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> customFieldMappings in customFieldValues)
                {
                    foreach (KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customField in customFieldMappings.Value)
                    {
                        SdtBlock contentBlock = contentBlockTemplate.CloneNode(true) as SdtBlock;

                        CustomFieldValueDTO fieldValue = customField.Key;
                        CustomFieldDTO field = customField.Value;

                        SdtElement customFieldLabel = WordUtilities.GetTaggedChildElement(contentBlock, FieldName_CustomFieldLabel);
                        SdtElement customFieldID = WordUtilities.GetTaggedChildElement(contentBlock, FieldName_CustomFieldID);
                        SdtElement customFieldDesc = WordUtilities.GetTaggedChildElement(contentBlock, FieldName_CustomFieldDesc);

                        // Note: The template has an indeterminate number of [multiple] Run elements containing "pieces" of the full text.
                        // Delete all but the first Run.
                        this.RemoveAllButOneElement<Run>(customFieldLabel.GetFirstChild<SdtContentRun>());
                        if (customFieldID != null)
                        {
                            this.RemoveAllButOneElement<Run>(customFieldID.GetFirstChild<SdtContentRun>());
                        }
                        this.RemoveAllButOneElement<Run>(customFieldDesc.GetFirstChild<SdtContentRun>());

                        // set values
                        WordUtilities.SetElementText(customFieldLabel, field.CustomFieldName);
                        if (customFieldID == null)
                        {
                            // Handle templates without CustomFieldID element (i.e. compatible with open-ended fields) as follows:  
                            // if open-ended, only show custom field description; otherwise concatenate custom field "name - description" in description element.
                            string customFieldString = field.IsOpenEnded ?
                                $"{fieldValue.CustomFieldValueDescription}" :
                                $"{fieldValue.CustomFieldValueName} - {fieldValue.CustomFieldValueDescription}";
                            WordUtilities.SetElementText(customFieldDesc, customFieldString);
                        }
                        else
                        {
                            // Handle old-style templates with CustomFieldID element and trailing dash "-" as follows:
                            // if open-ended, set CustomFieldID to empty string; otherwise display name and description in their respective elements.
                            WordUtilities.SetElementText(customFieldID, field.IsOpenEnded ? string.Empty : fieldValue.CustomFieldValueName);
                            WordUtilities.SetElementText(customFieldDesc, fieldValue.CustomFieldValueDescription);
                        }

                        // add the row to the table
                        currentInsertionBlock.InsertAfterSelf(contentBlock);
                        currentInsertionBlock = contentBlock;
                    }
                }

                contentBlockTemplate.RemoveIt();
            }
            else
            {
                customFieldsElement.RemoveIt();
            }
        }

        /// <summary>
        /// Removes all but the first element
        /// </summary>
        /// <typeparam name="T">type of element being removed</typeparam>
        /// <param name="element">element elements are being removed from</param>
        /// <param name="removeElementTypeOnly">Bool if only element type is to be removed</param>
        private void RemoveAllButOneElement<T>(OpenXmlElement element, bool removeElementTypeOnly = true) where T : OpenXmlElement
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            bool found = false;

            OpenXmlElement[] allChildElements = (removeElementTypeOnly ? element.Descendants<T>() : element.Descendants()).ToArray();

            for (int i = 0; i < allChildElements.Length; i++)
            {
                OpenXmlElement childElement = allChildElements[i];

                if (childElement is T)
                {
                    if (!found)
                    {
                        found = true;
                    }
                    else
                    {
                        this.RemoveElement(childElement);
                    }
                }
                else if (!removeElementTypeOnly)
                {
                    this.RemoveElement(childElement);
                }
            }
        }

        /// <summary>
        /// Derive a lookup table of custom field names and values for each of the task's resource entries.
        /// </summary>
        /// <param name="customFieldValueIdMappings">Maps each resource entry (labor type id) to its custom field assignments</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// Table of custom field names and values, indexed by labor type id
        /// </returns>
        private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetTaskElementCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings, BOEExportInputs exportInputs)
        {
            IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> taskElementCustomFields = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();
            IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields = exportInputs.CustomFields;

            foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> laborTaskCustomFieldIdMapping in customFieldValueIdMappings)
            {
                int laborTypeId = laborTaskCustomFieldIdMapping.Key;
                ICollection<KeyValuePair<int, int>> idPairs = laborTaskCustomFieldIdMapping.Value;

                // Get all custom field values for the task element.
                ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
                ICollection<CustomFieldValueDTO> customFieldValues = exportInputs.CustomFieldValues.Where(x => customFieldValueIds.Contains(x.Id)).ToList();

                foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues.OrderBy(x => x.CustomFieldID))
                {
                    CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
                    if (customFieldDto != null)
                    {
                        IDictionary<CustomFieldValueDTO, CustomFieldDTO> currentLaborTypeCustomFields;
                        if (taskElementCustomFields.ContainsKey(laborTypeId))
                        {
                            currentLaborTypeCustomFields = taskElementCustomFields[laborTypeId];
                        }
                        else
                        {
                            currentLaborTypeCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
                            taskElementCustomFields.Add(laborTypeId, currentLaborTypeCustomFields);
                        }

                        // add to list
                        currentLaborTypeCustomFields.Add(customFieldValueDto, customFieldDto);
                    }
                }
            }

            return taskElementCustomFields;
        }

        /// <summary>
        /// Gets resource-level custom fields
        /// </summary>
        /// <param name="customFieldValueIdMappings">Mappings for the custom field value ids</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetResourceCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings, BOEExportInputs exportInputs)
        {
            IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> resourceCustomFields = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();
            IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields = exportInputs.CustomFields;

            foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> resourceCustomFieldIdMapping in customFieldValueIdMappings)
            {
                int resourceId = resourceCustomFieldIdMapping.Key;
                ICollection<KeyValuePair<int, int>> idPairs = resourceCustomFieldIdMapping.Value;

                // Get all custom field values for the task element.
                ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
                ICollection<CustomFieldValueDTO> customFieldValues = exportInputs.CustomFieldValues.Where(x => customFieldValueIds.Contains(x.Id)).ToList();

                foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues.OrderBy(x => x.CustomFieldID))
                {
                    CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
                    if (customFieldDto != null)
                    {
                        IDictionary<CustomFieldValueDTO, CustomFieldDTO> currentResourceCustomFields;
                        if (resourceCustomFields.ContainsKey(resourceId))
                        {
                            currentResourceCustomFields = resourceCustomFields[resourceId];
                        }
                        else
                        {
                            currentResourceCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
                            resourceCustomFields.Add(resourceId, currentResourceCustomFields);
                        }

                        // add to list
                        currentResourceCustomFields.Add(customFieldValueDto, customFieldDto);
                    }
                }
            }

            return resourceCustomFields;
        }

        /// <summary>
        /// Populates the resource summary by resource ids in the form of a table, sets up rollup data
        /// </summary>
        /// <param name="element">Element to set</param>
        /// <param name="boeExportModelView">Export model view for the BOE</param>
        private void PopulateResourceSummaryByResourceIDTable(SdtElement element, BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            if (boeExportModelView.TaskElements.Any(x => x.taskElementLabors.Any()))
            {
                ICollection<ResourceSummaryRowData> resourceData = this.GetTaskElementResourceData(boeExportModelView);

                List<ResourceSummaryRowData> rollupData =
                    resourceData
                        .GroupBy(x => x.GroupKey)
                        .Select(g => new ResourceSummaryRowData
                        {
                            ResourceType = g.First().ResourceType,
                            ResourceName = g.First().ResourceName,
                            CostTotal = g.Sum(x => x.CostTotal),
                            HoursTotal = g.Sum(x => x.HoursTotal)
                        })
                        .OrderBy(x => x.GroupKey).ToList();

                this.PopulateResourceSummaryTable(element, rollupData);
            }
            else
            {
                element.RemoveIt();
            }
        }

        /// <summary>
        /// Populate the version of the Resource Summary By Resource ID table that includes Labor Category and Location custom fields for RMS
        /// </summary>
        /// <param name="element">Element to set</param>
        /// <param name="boeExportModelView">Export model view for the BOE</param>
        /// <param name="exportInputs">Export inputs</param>
        private void PopulateResourceSummaryByResourceIDLaborCategoryLocationTable(SdtElement element, BOEExportModelView boeExportModelView, BOEExportInputs exportInputs)
        {
            _ = boeExportModelView ?? throw new ArgumentNullException(nameof(boeExportModelView));

            if (boeExportModelView.TaskElements.Any(x => x.taskElementLabors.Any()))
            {
                ICollection<ResourceSummaryRowData> resourceData = this.GetTaskElementResourceDataWithLaborCategoryAndLocation(boeExportModelView, exportInputs);

                List<ResourceSummaryRowData> rollupData =
                    resourceData
                        .GroupBy(x => x.GroupKeyWithLaborCategoryAndLocation)
                        .Select(g => new ResourceSummaryRowData
                        {
                            ResourceType = g.First().ResourceType,
                            ResourceName = g.First().ResourceName,
                            HoursTotal = g.Sum(x => x.HoursTotal),
                            LaborCategory = g.First().LaborCategory,
                            Location = g.First().Location
                        })
                        .OrderBy(x => x.GroupKeyWithLaborCategoryAndLocation).ToList();

                this.PopulateResourceSummaryTable(element, rollupData);
            }
            else
            {
                element.RemoveIt();
            }
        }

        /// <summary>
        /// Gets the resource data for the task elements using the task element labors of the export modelview
        /// </summary>
        /// <param name="boeExportModelView">BOE Export ModelView containing the task element labors</param>
        /// <returns>Collection of Resource Data</returns>
        protected virtual ICollection<ResourceSummaryRowData> GetTaskElementResourceData(BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            ICollection<ResourceSummaryRowData> resourceData = new Collection<ResourceSummaryRowData>();

            foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.SelectMany(t => t.taskElementLabors))
            {
                resourceData.Add(new ResourceSummaryRowData()
                {
                    ResourceType = taskElementLabor.ExportFields[FieldName_ResourceElementOfCost],
                    ResourceName = taskElementLabor.ExportFields[FieldName_TaskTypeDescription],
                    CostTotal = taskElementLabor.Cost.HasValue ? taskElementLabor.Cost.Value : 0m,
                    HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m
                });
            }

            return resourceData;
        }

        /// <summary>
        /// Gets the resource data for the task elements using the task element labors of the export modelview
        /// </summary>
        /// <param name="boeExportModelView">BOE Export ModelView containing the task element labors</param>
        /// <param name="exportInputs">Export Inputs</param>
        /// <returns>Collection of Resource Data</returns>
        protected virtual ICollection<ResourceSummaryRowData> GetTaskElementResourceDataWithLaborCategoryAndLocation(BOEExportModelView boeExportModelView, BOEExportInputs exportInputs)
        {
            _ = boeExportModelView ?? throw new ArgumentNullException(nameof(boeExportModelView));

            ICollection<ResourceSummaryRowData> resourceData = new Collection<ResourceSummaryRowData>();

            // This table only displays hours and custom fields, so Cost labors are excluded
            foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.SelectMany(t => t.taskElementLabors).Where(x => x.Hours.HasValue))
            {
                // The custom field values for Labor Category and Location are stored in the Export Fields using GovtLaborCategory
                // and KeyPersonnel from a previous item
                resourceData.Add(new ResourceSummaryRowData()
                {
                    ResourceType = taskElementLabor.ExportFields[FieldName_ResourceElementOfCost],
                    ResourceName = taskElementLabor.ExportFields[FieldName_TaskTypeDescription],
                    HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m,
                    LaborCategory = taskElementLabor.ExportFields[FieldName_GovtLaborCategory],
                    Location = taskElementLabor.ExportFields[FieldName_KeyPersonnel]
                });
            }

            return resourceData;
        }

        /// <summary>
        /// Pupulates resource summary table with rollupData
        /// </summary>
        /// <param name="element">Element to set</param>
        /// <param name="rollupData">data to use to populate table</param>
        private void PopulateResourceSummaryTable(SdtElement element, List<ResourceSummaryRowData> rollupData)
        {
            if (rollupData != null && rollupData.Any())
            {
                // locate the table markers
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(element, BOEExporterConstants.Marker_DataRow);
                SdtElement dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(element, BOEExporterConstants.Marker_TotalsRow);

                // initialize the "insertion" row
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                if (templateDataRow != null)
                {
                    TableRow currentInsertionRow = templateDataRow;

                    // accumulate totals
                    decimal matSubIwtaCostTotal = 0m;
                    decimal otherCostTotal = 0m;
                    decimal hoursTotal = 0m;
                    decimal costTotal = 0m;
                    decimal monthsTotal = 0m;

                    foreach (ResourceSummaryRowData rollupRowData in rollupData)
                    {
                        // create a new summary data row in the table
                        // clone marked template row
                        TableRow tableRow = templateDataRow.CloneNode(true) as TableRow;
                        foreach (SdtId id in tableRow.Descendants<SdtId>())
                        {
                            id.RemoveIt();
                        }

                        foreach (SdtPlaceholder placeholder in tableRow.Descendants<SdtPlaceholder>())
                        {
                            placeholder.RemoveIt();
                        }

                        decimal hours = rollupRowData.HoursTotal.HasValue ? rollupRowData.HoursTotal.Value : 0m;
                        hoursTotal += hours;

                        decimal months = 0m;
                        if (rollupRowData.StartDate.HasValue && rollupRowData.EndDate.HasValue)
                        {
                            months = ((rollupRowData.EndDate.Value.Year - rollupRowData.StartDate.Value.Year) * 12) + rollupRowData.EndDate.Value.Month - rollupRowData.StartDate.Value.Month; // equation to get months between dates
                        }

                        monthsTotal += months;

                        decimal matSubIwtaCost = 0m;
                        decimal otherCost = 0m;
                        decimal cost = rollupRowData.CostTotal.HasValue ? rollupRowData.CostTotal.Value : 0m;
                        if (rollupRowData.ResourceType == "Materials" || rollupRowData.ResourceType == "Sub" || rollupRowData.ResourceType == "IWTA")
                        {
                            matSubIwtaCost = cost;
                        }
                        else if (rollupRowData.ResourceType == "ODC" || rollupRowData.ResourceType == "Travel")
                        {
                            otherCost = cost;
                        }

                        matSubIwtaCostTotal += matSubIwtaCost;
                        otherCostTotal += otherCost;
                        costTotal += cost;

                        // populate the row
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rollupRowData.ResourceName);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), Utilities.FormatStringWithPrecision(hours, this.WorkspaceDecimalPrecision));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Months), months.ToString(BOEExporterConstants.NUMERIC_FORMAT_COMMAS_NO_DECIMALS));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_MatSubIWTACost), matSubIwtaCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_OtherCost), otherCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cost), cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, LABOR_CATEGORY_CUSTOM_FIELD), rollupRowData.LaborCategory);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, LOCATION_CUSTOM_FIELD), rollupRowData.Location);

                        // add the row to the table
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }

                    // total row doesn't need to be cloned
                    TableRow totalsRow = dataTotalsRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();

                    // populate the overall totals
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_HoursTotal), Utilities.FormatStringWithPrecision(hoursTotal, this.WorkspaceDecimalPrecision));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_MonthsTotal), monthsTotal.ToString(BOEExporterConstants.NUMERIC_FORMAT_COMMAS_NO_DECIMALS));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_MatSubIWTACostTotal), matSubIwtaCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_OtherCostTotal), otherCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), costTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));

                    // remove template rows
                    templateDataRow.RemoveIt();
                }
            }
            else
            {
                element.RemoveIt();
            }
        }

        /// <summary>
        /// Process the data for the Project Map resource summary in the form of a table, sets up data
        /// </summary>
        /// <param name="element">Element to set</param>
        /// <param name="boeExportModelView">Export model view for the BOE</param>
        private void ProcessProjectMapResourceSummaryTable(SdtElement element, BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            if (boeExportModelView.TaskElements.Any(x => x.taskElementLabors.Any()))
            {
                List<ProjectMapResourceSummaryRowData> resourceData = this.GetProjectMapTaskElementResourceData(boeExportModelView).OrderBy(x => x.ResourceCode).ThenBy(y => y.CostCenterCode).ToList();
                this.PopulateProjectMapResourceSummaryTable(element, resourceData);
            }
            else
            {
                element.RemoveIt();
            }
        }

        /// <summary>
        /// Gets the resource data for the task elements for the Project Map Resource Summary using the task element labors of the export modelview
        /// </summary>
        /// <param name="boeExportModelView">BOE Export ModelView containing the task element labors</param>
        /// <returns>Collection of Resource Data</returns>
        private ICollection<ProjectMapResourceSummaryRowData> GetProjectMapTaskElementResourceData(BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            ICollection<ProjectMapResourceSummaryRowData> resourceData = new Collection<ProjectMapResourceSummaryRowData>();

            foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.SelectMany(t => t.taskElementLabors))
            {
                resourceData.Add(new ProjectMapResourceSummaryRowData()
                {
                    ResourceCode = taskElementLabor.ExportFields[FieldName_ResourceName],
                    CostCenterCode = taskElementLabor.ExportFields[FieldName_PerfOrg],
                    StartDate = taskElementLabor.StartDate,
                    EndDate = taskElementLabor.EndDate,
                    CostTotal = taskElementLabor.Cost.HasValue ? taskElementLabor.Cost.Value : 0m,
                    HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m,
                    TieredPercent = taskElementLabor.TieredPercent
                });
            }

            return resourceData;
        }

        /// <summary>
        /// Populates the Project Map Resource Summary Table
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="resourceData">resource data to populate the table with</param>
        private void PopulateProjectMapResourceSummaryTable(SdtElement element, List<ProjectMapResourceSummaryRowData> resourceData)
        {
            if (resourceData != null && resourceData.Any())
            {
                // locate the table markers
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(element, BOEExporterConstants.Marker_DataRow);

                // initialize the "insertion" row
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                if (templateDataRow != null)
                {
                    TableRow currentInsertionRow = templateDataRow;

                    foreach (ProjectMapResourceSummaryRowData rowData in resourceData)
                    {
                        // create a new summary data row in the table
                        // clone marked template row
                        TableRow tableRow = templateDataRow.CloneNode(true) as TableRow;
                        foreach (SdtId id in tableRow.Descendants<SdtId>())
                        {
                            id.RemoveIt();
                        }

                        foreach (SdtPlaceholder placeholder in tableRow.Descendants<SdtPlaceholder>())
                        {
                            placeholder.RemoveIt();
                        }

                        decimal hours = rowData.HoursTotal.HasValue ? rowData.HoursTotal.Value : 0m;
                        decimal cost = rowData.CostTotal.HasValue ? rowData.CostTotal.Value : 0m;

                        string startDate = rowData.StartDate.HasValue ? ((DateTime)rowData.StartDate).ToString("MM/yyyy") : string.Empty;
                        string endDate = rowData.EndDate.HasValue ? ((DateTime)rowData.EndDate).ToString("MM/yyyy") : string.Empty;

                        // populate the row
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceID), rowData.ResourceCode);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_PerfOrg), rowData.CostCenterCode);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceStartDate), startDate);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_ResourceEndDate), endDate);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_LaborTypeHours), Utilities.FormatStringWithPrecision(hours, this.WorkspaceDecimalPrecision));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_LaborTypeCost), cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this.CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, FieldName_TieredPercent), rowData.TieredPercent.HasValue ? rowData.TieredPercent.Value.ToString("F1") + " %" : string.Empty);

                        // add the row to the table
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }

                    // remove template rows
                    templateDataRow.RemoveIt();
                }
            }
            else
            {
                element.RemoveIt();
            }
        }

        /// <summary>
        /// Populates the BOE-Level Summary Table
        /// </summary>
        /// <param name="element">Element containing the table</param>
        /// <param name="boeExportModelView">BOE Export MV</param>
        /// <param name="exportInputs">BOE Export Inputs</param>
        private void PopulateBOELevelSummaryTable(SdtElement element, BOEExportModelView boeExportModelView, BOEExportInputs exportInputs)
        {
            // get table and row elements
            Table table = element.Descendants<Table>().First();
            TableRow headerRow = table.Descendants<TableRow>().First();
            SdtElement yearValueMarker = WordUtilities.GetTaggedChildElement(element, FieldName_YearValue);
            TableRow valueRow = yearValueMarker.Ancestors<TableRow>().First();

            ICollection<BoeTaskElementDTO> taskElements = exportInputs.TaskElements.Where(x => x.BoeID == boeExportModelView.BoeID).ToCollection();

            if (taskElements.Any())
            {
                ICollection<ResourceTypeDto> taskElementLabors = taskElements.SelectMany(x => x.taskElementLabors).ToCollection();
                if (taskElementLabors.Any())
                {
                    ICollection<ResourceSpreadDto> spreads = taskElementLabors.SelectMany(x => x.LaborSpreads).ToCollection();
                    if (spreads.Any())
                    {
                        // populate column for first year
                        int firstYear = boeExportModelView.StartDate.Year;
                        decimal firstYearValue = spreads.Where(x => x.LaborSpreadDate.Year == firstYear)
                            .Sum(y => y.LaborSpreadValue);
                        WordUtilities.SetElementText(
                            WordUtilities.GetTaggedChildElement(headerRow, FieldName_CalendarYear),
                            "CY " + firstYear.ToString());
                        WordUtilities.SetElementText(yearValueMarker, Utilities.FormatStringWithPrecision(firstYearValue, this.WorkspaceDecimalPrecision));

                        // populate remaing year columns
                        for (int year = firstYear + 1; year <= boeExportModelView.EndDate.Year; year++)
                        {
                            decimal yearValue = spreads.Where(x => x.LaborSpreadDate.Year == year)
                                .Sum(y => y.LaborSpreadValue);

                            this.AppendCellToRow(headerRow, "CY " + year.ToString());
                            this.AppendCellToRow(valueRow, Utilities.FormatStringWithPrecision(yearValue, this.WorkspaceDecimalPrecision));
                        }

                        // populate total column
                        this.AppendCellToRow(headerRow, "Total");
                        this.AppendCellToRow(valueRow, Utilities.FormatStringWithPrecision(taskElements.Sum(x => x.TotalHours ?? 0), this.WorkspaceDecimalPrecision));
                    }
                }
            }
        }

        /// <summary>
        /// Populates the task cost rollup table
        /// </summary>
        /// <param name="Rollup">Labor rollup data by date</param>
        /// <param name="Font">Font to use</param>
        /// <param name="TableAlias">Alias of the table</param>
        /// <param name="taskContainer">task container containing rollup</param>
        /// <param name="containerName">name of container</param>
        private void PopulateTaskCostRollup(Collection<LaborRollupByDate> Rollup, string Font, SdtAlias TableAlias, BOEExportTaskContainer taskContainer, string containerName)
        {
            NumberFormatInfo currencyFormatter = new NumberFormatInfo();
            currencyFormatter.CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR;
            currencyFormatter.CurrencySymbol = string.Empty;
            currencyFormatter.CurrencyDecimalDigits = 2;

            if (Rollup != null && Rollup.Any())
            {
                string format = BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS;
				SdtElement element = TableAlias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    this.PopulateBoeYearSummaryRollup(element, Rollup.ToCollection(), format, this.CurrencyFormatter, Font, "18", "18", TableAlias.Val.Value, JustificationValues.Center);
                }
            }
            else
            {
                SdtAlias alias = taskContainer.TaskContainer.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals(containerName, StringComparison.CurrentCultureIgnoreCase));

				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();
                if (element != null)
                {
                    element.RemoveAllChildren();
                }

                alias.RemoveIt();
            }
        }

        /// <summary>
        /// Populates the SOW Resource Table
        /// </summary>
        /// <param name="exportInputs">BOE Export Inputs</param>
        /// <param name="tableAlias">table alias</param>
        /// <param name="boeExportModelView">the boe export modelview</param>
        /// <param name="resourceTypeDtos">Resource Type DTOs</param>
        private void PopulateSOWResourceTable(BOEExportInputs exportInputs, SdtAlias tableAlias, BOEExportModelView boeExportModelView,
            ICollection<ResourceTypeDto> resourceTypeDtos)
        {
            // Get table and row elements
            SdtElement tableElement = tableAlias.Ancestors<SdtElement>().First();
            Table table = tableElement.Descendants<Table>().First();
            TableRow headerRow = table.Descendants<TableRow>().First();
            SdtElement dataRowMarkerTag =
                WordUtilities.GetTaggedChildElement(tableElement, BOEExporterConstants.Marker_DataRow);
            TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().First();
            this.SetCantSplit(templateDataRow);
            TableRow currentInsertionRow = templateDataRow;
            SdtElement totalRowMarkerTag =
                WordUtilities.GetTaggedChildElement(tableElement, BOEExporterConstants.Marker_TotalsRow);
            TableRow totalsRow = totalRowMarkerTag.Ancestors<TableRow>().First();

            int startYear = boeExportModelView.StartDate.Year;
            int endYear = boeExportModelView.EndDate.Year;

            IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields = exportInputs.CustomFields;
            IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues = exportInputs.CustomFieldValues;
            CustomFieldDTO sowCustomField = workspaceCustomFields.FirstOrDefault(x => x.CustomFieldName == BOEExporterConstants.CustomFieldName_SOW);
            int sowCustomFieldID = sowCustomField == null ? -1 : sowCustomField.Id;

            // populate header row
            SdtElement firstYearHeader = WordUtilities.GetTaggedChildElement(headerRow, FieldName_CalendarYear);
            WordUtilities.SetElementText(firstYearHeader, "CY " + startYear);
            for (int year = startYear + 1; year <= endYear; year++)
            {
                this.AppendCellToRow(headerRow, "CY " + year);
            }

            // group Resource Types by SOW and Resource ID
            var groupedResources = resourceTypeDtos.GroupBy(x => new
            {
                sow = workspaceCustomFieldValues.Where(c => c.CustomFieldValueID == x.CustomFieldValueContainers.Where(y => y.CustomFieldID == sowCustomFieldID).Select(z => z.CustomFieldValueID).FirstOrDefault()).Select(n => n.CustomFieldValueName).FirstOrDefault(),
                laborCategory = x.ResourceID
            }).OrderBy(o => o.Key.sow).ThenBy(t => t.Key.laborCategory);

            ////string previousSow = string.Empty;

            // populate data rows
            foreach (var resourceGroup in groupedResources)
            {
                TableRow row = this.CloneMarkedTemplateRow(templateDataRow);
                
                string sow = resourceGroup.Key.sow;
                int laborCategoryID = resourceGroup.Key.laborCategory ?? -1;                
                ResourceDTO laborCategoryDto =
                    exportInputs.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == laborCategoryID);
                string laborCategory = laborCategoryDto.ResourceDesc;

                ICollection<ResourceSpreadDto> spreads = resourceGroup.SelectMany(x => x.LaborSpreads).ToCollection();
                decimal startYearValue = spreads.Where(x => x.LaborSpreadDate.Year == startYear)
                    .Sum(y => y.LaborSpreadValue);

                SdtElement sowElement = WordUtilities.GetTaggedChildElement(row, FieldName_SOW);
                TableCell cell = sowElement.Ancestors<TableCell>().FirstOrDefault();

                WordUtilities.SetElementText(sowElement, sow);

                if (cell != null && cell.TableCellProperties != null)
                {
                    // Set this so cells below it can merge vertically if needed
                    VerticalMerge verticalMerge = new VerticalMerge() {Val = MergedCellValues.Restart};
                    cell.TableCellProperties.Append(verticalMerge);
                }

                WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(row, FieldName_LaborCategory),
                    laborCategory);
                WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(row, FieldName_YearValue),
                    Utilities.FormatStringWithPrecision(startYearValue, this.WorkspaceDecimalPrecision));

                // add values for remaining years
                for (int year = startYear + 1; year <= endYear; year++)
                {
                    decimal yearValue = spreads.Where(x => x.LaborSpreadDate.Year == year).Sum(y => y.LaborSpreadValue);
                    this.AppendCellToRow(row,
                        Utilities.FormatStringWithPrecision(yearValue, this.WorkspaceDecimalPrecision));
                }

                currentInsertionRow.InsertAfterSelf(row);
                currentInsertionRow = row;
            }

            // remove template row
            templateDataRow.Remove();

            // populate totals row
            ICollection<ResourceSpreadDto> resourceSpreads =
                resourceTypeDtos.SelectMany(x => x.LaborSpreads).ToCollection();
            decimal startYearTotal = resourceSpreads.Where(x => x.LaborSpreadDate.Year == startYear)
                .Sum(y => y.LaborSpreadValue);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, FieldName_YearValueTotal),
                Utilities.FormatStringWithPrecision(startYearTotal, this.WorkspaceDecimalPrecision));
            for (int year = startYear + 1; year <= endYear; year++)
            {
                decimal yearTotal = resourceSpreads.Where(x => x.LaborSpreadDate.Year == year)
                    .Sum(y => y.LaborSpreadValue);
                this.AppendCellToRow(totalsRow, Utilities.FormatStringWithPrecision(yearTotal, this.WorkspaceDecimalPrecision));
            }
        }

        /// <summary>
        /// Populates the year summary rollup table
        /// </summary>
        /// <param name="element">element to set</param>
        /// <param name="laborRollup">labor rollup data</param>
        /// <param name="Format">Format to use</param>
        /// <param name="NumberFormatter">Format for numbers</param>
        /// <param name="Font">Font to use</param>
        /// <param name="FontSize">Font size to use</param>
        /// <param name="headerFontSize">Font size for the table header</param>
        /// <param name="tag">Content control tag name</param>
        /// <param name="numberAlignment">Horizontal text alignment for number values in the table</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        protected virtual void PopulateBoeYearSummaryRollup(SdtElement element, Collection<LaborRollupByDate> laborRollup, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, string headerFontSize, string tag, JustificationValues numberAlignment)
        {
            if (element != null)
            {
                element.RemoveAllChildren();

                // Create the table
                Table table;
                bool noAfterSpacing;
                if (tag == FieldName_GSMOSummaryHourByDate)
                {
                    table = this.CreateGSMORollupTable(TableLayoutValues.Autofit);
                    noAfterSpacing = true;
                }
                else
                {
                    table = this.CreateRollupTable();
                    noAfterSpacing = false;
                }

                // Create the header row
                List<string> Headers = new List<string>() { "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };
                table.Append(this.CreateRollupHeaderRow(Headers, Font, headerFontSize, noAfterSpacing));

                ParagraphProperties centerPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new SpacingBetweenLines() { After = "0" });
                ParagraphProperties rightPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Right }, new SpacingBetweenLines() { After = "0" });
                ParagraphProperties naPP = new ParagraphProperties(new Justification() { Val = numberAlignment }, new SpacingBetweenLines() { After = "0" });

                if (laborRollup != null)
                {
                    foreach (LaborRollupByDate item in laborRollup)
                    {
						TableRow tr2 = new TableRow();

                        // so rows won't be split across pages
                        TableRowProperties trp = new TableRowProperties();
                        CantSplit cantSplit = new CantSplit();
                        trp.Append(cantSplit);
                        tr2.Append(trp);

                        TableCellProperties tcp = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                        this.PopulateTableCell(tr2, item.Year.ToString(), Font, FontSize, tcp, centerPP);
                        this.PopulateTableCell(tr2, item.January.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.February.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.March.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.April.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.May.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.June.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.July.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.August.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.September.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.October.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.November.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr2, item.December.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = "$";
                        }
                        this.PopulateTableCell(tr2, item.Total.ToString(Format, NumberFormatter), Font, FontSize, tcp, naPP);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = string.Empty;
                        }

                        table.Append(tr2);
                    }
                }

				TableRow tr3 = new TableRow();

                TableCellProperties totalLabelProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                    new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }), new TableCellBorders(new LeftBorder() { Val = BorderValues.Nil }));
                RunProperties totalLabelRunProperties = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                this.PopulateTableCell(tr3, "Total\u00A0", Font, FontSize, totalLabelProperties, rightPP, totalLabelRunProperties, new GridSpan() { Val = 13 });

                if (NumberFormatter != null)
                {
                    NumberFormatter.CurrencySymbol = "$";
                }
                TableCellProperties totalValueProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                    new TableCellBorders(new BottomBorder() { Val = BorderValues.Single, Color = "auto", Size = (UInt32Value)4U, Space = (UInt32Value)0U }));
                this.PopulateTableCell(tr3, laborRollup.Select(x => x.Total).Sum().ToString(Format, NumberFormatter), Font, FontSize, totalValueProperties, naPP);
                if (NumberFormatter != null)
                {
                    NumberFormatter.CurrencySymbol = string.Empty;
                }

                table.Append(tr3);
                element.Append(table);
            }
        }

        /// <summary>
        /// Populates the task element rollup table
        /// </summary>
        /// <param name="element">Table element</param>
        /// <param name="taskRollup">Rollup data for the table</param>
        /// <param name="TasksDateRange">Date range of tasks in the table</param>
        /// <param name="Format">toString format for rollup data value strings in the table</param>
        /// <param name="NumberFormatter">toString format information to go with Format</param>
        /// <param name="Font">Font for table text</param>
        /// <param name="FontSize">Font size for non-header text in the table</param>
        /// <param name="headerFontSize">Font size for text in the header row of the table</param>
        /// <param name="numberAlignment">Horizontal text alignment for number values in the table</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateTaskElementRollup(SdtElement element, Dictionary<int, List<LaborRollupByDate>> taskRollup,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, string headerFontSize, JustificationValues numberAlignment, 
            bool includesCompanyAndLocation = false)
        {
            if (element != null)
            {
                bool gsmo = false;
                if (element.Descendants<SdtAlias>().Any(x => x.Val.Value.Contains("GSMO") || x.Val.Value.Contains("SMORS")))
                {
                    // also applies to SMORS
                    gsmo = true;
                }

                element.RemoveAllChildren();

                // Create the table
                Table table;
                bool noAfterSpacing;
                if (gsmo)
                {
                    table = this.CreateGSMORollupTable(TableLayoutValues.Autofit);
                    noAfterSpacing = true;
                }
                else
                {
                    table = this.CreateRollupTable();
                    noAfterSpacing = false;
                }

                // Create the header row
                List<string> Headers = new List<string>() { "Resource", "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };

                if (includesCompanyAndLocation)
                {
                    Headers.Insert(1, "Company");
                    Headers.Insert(2, "Location");
                    Headers.Insert(3, "Perf Org");
                }

                table.Append(this.CreateRollupHeaderRow(Headers, Font, headerFontSize, noAfterSpacing));

                if (!taskRollup.Keys.Any())
                {
                    table.Append(this.CreateTableBlankRow(15));
                }

                ParagraphProperties naPP = new ParagraphProperties(new Justification() { Val = numberAlignment }, new SpacingBetweenLines() { After = "0" });
                ParagraphProperties centerPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new SpacingBetweenLines() { After = "0" });
                ParagraphProperties rightPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Right }, new SpacingBetweenLines() { After = "0" });

                // Create the year rows for each resource
                foreach (int item in taskRollup.Keys)
                {
                    foreach (LaborRollupByDate item2 in taskRollup[item])
                    {
                        TableRow tr2 = new TableRow();

                        TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                        this.PopulateTableCell(tr2, item2.Resource, Font, FontSize, cellProperties, centerPP);

                        if (includesCompanyAndLocation)
                        {
                            this.PopulateTableCell(tr2, item2.Company, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr2, item2.Location, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr2, item2.PerfOrg, Font, FontSize, cellProperties, naPP);
                        }

                        this.PopulateTableCell(tr2, item2.Year.ToString(), Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr2, item2.January.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.February.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.March.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.April.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.May.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.June.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.July.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.August.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.September.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.October.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.November.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr2, item2.December.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        if (NumberFormatter != null) { NumberFormatter.CurrencySymbol = "$"; }
                        this.PopulateTableCell(tr2, item2.Total.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                        if (NumberFormatter != null) { NumberFormatter.CurrencySymbol = string.Empty; }

                        table.Append(tr2);
                    }

                    TableRow tr3 = new TableRow();

                    // print the Total label
                    TableCellProperties totalLabelCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                        new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }));
                    RunProperties totalLabelRunProperties = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    
                    this.PopulateTableCell(tr3, "Total\u00A0", Font, FontSize, totalLabelCellProperties, rightPP, totalLabelRunProperties, new GridSpan() { Val = includesCompanyAndLocation ? 17 : 14 });

                    // print the Total value
                    TableCellProperties totalValueCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                        new TableCellBorders(new BottomBorder() { Val = BorderValues.Single, Color = "auto", Size = (UInt32Value)4U, Space = (UInt32Value)0U }));
                    if (NumberFormatter != null) { NumberFormatter.CurrencySymbol = "$"; }
                    this.PopulateTableCell(tr3, taskRollup[item].Select(x => x.Total).Sum().ToString(Format, NumberFormatter), Font, FontSize, totalValueCellProperties, naPP);
                    if (NumberFormatter != null) { NumberFormatter.CurrencySymbol = string.Empty; }

                    table.Append(tr3);

                    // blank row
                    TableRow tr4 = new TableRow();
                    TableCellProperties blankCellProperties = new TableCellProperties(new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center },
                        new TableCellWidth() { Type = TableWidthUnitValues.Pct }), new TableCellBorders(new TopBorder() { Val = BorderValues.Nil }));
                    RunProperties blankRunProperties = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    this.PopulateTableCell(tr4, string.Empty, Font, FontSize, blankCellProperties, centerPP, blankRunProperties, new GridSpan() { Val = includesCompanyAndLocation ? 18 : 15 });

                    table.Append(tr4);
                }

                if (taskRollup.Keys.Any())
                {
                    // Summary
                    if (TasksDateRange.StartDate.HasValue && TasksDateRange.EndDate.HasValue)
                    {
                        List<LaborRollupByDate> LaborValues = taskRollup.Values.SelectMany(x => x).ToList();

                        for (int i = TasksDateRange.StartDate.Value.Year; i <= TasksDateRange.EndDate.Value.Year; i++)
                        {
                            // create shaded Summary label cell
                            TableRow tr5 = new TableRow();
                            TableCellProperties shadedCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center },
                                new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                            RunProperties shadedRunProperties = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                            this.PopulateTableCell(tr5, "Summary", Font, FontSize, shadedCellProperties, centerPP, shadedRunProperties);

                            // get total values for year
                            string janSum = LaborValues.Where(x => x.Year == i).Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                            string febSum = LaborValues.Where(x => x.Year == i).Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                            string marSum = LaborValues.Where(x => x.Year == i).Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                            string aprSum = LaborValues.Where(x => x.Year == i).Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                            string maySum = LaborValues.Where(x => x.Year == i).Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                            string junSum = LaborValues.Where(x => x.Year == i).Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                            string julSum = LaborValues.Where(x => x.Year == i).Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                            string augSum = LaborValues.Where(x => x.Year == i).Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                            string sepSum = LaborValues.Where(x => x.Year == i).Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                            string octSum = LaborValues.Where(x => x.Year == i).Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                            string novSum = LaborValues.Where(x => x.Year == i).Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                            string decSum = LaborValues.Where(x => x.Year == i).Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                            if (NumberFormatter != null)
                            {
                                NumberFormatter.CurrencySymbol = "$";
                            }
                            string totalSum = LaborValues.Where(x => x.Year == i).Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                            if (NumberFormatter != null)
                            {
                                NumberFormatter.CurrencySymbol = string.Empty;
                            }

                            //populate cells with above values
                            TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                            if (includesCompanyAndLocation)
                            {
                                this.PopulateTableCell(tr5, string.Empty, Font, FontSize, cellProperties, naPP);
                                this.PopulateTableCell(tr5, string.Empty, Font, FontSize, cellProperties, naPP);
                                this.PopulateTableCell(tr5, string.Empty, Font, FontSize, cellProperties, naPP);
                            }

                            this.PopulateTableCell(tr5, i.ToString(), Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, janSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, febSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, marSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, aprSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, maySum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, junSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, julSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, augSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, sepSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, octSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, novSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, decSum, Font, FontSize, cellProperties, naPP);
                            this.PopulateTableCell(tr5, totalSum, Font, FontSize, cellProperties, naPP);

                            table.Append(tr5);
                        }

                        // Create the totals row
                        TableRow tr6 = new TableRow();

                        // Shaded summary label
                        TableCellProperties summaryCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center },
                            new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                        RunProperties rpSummaryTotal = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                        this.PopulateTableCell(tr6, "Summary", Font, FontSize, summaryCellProperties, centerPP, rpSummaryTotal);

                        // Total label
                        TableCellProperties tcp = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
                        this.PopulateTableCell(tr6, "Total", Font, FontSize, tcp, centerPP, rpSummaryTotal, new GridSpan() { Val = includesCompanyAndLocation ? 4 : 1 });

                        // get overall total values
                        string janTotal = LaborValues.Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                        string febTotal = LaborValues.Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                        string marTotal = LaborValues.Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                        string aprTotal = LaborValues.Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                        string mayTotal = LaborValues.Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                        string junTotal = LaborValues.Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                        string julTotal = LaborValues.Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                        string augTotal = LaborValues.Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                        string sepTotal = LaborValues.Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                        string octTotal = LaborValues.Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                        string novTotal = LaborValues.Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                        string decTotal = LaborValues.Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = "$";
                        }
                        string totalTotal = LaborValues.Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = string.Empty;
                        }

                        // populate cells with total values
                        this.PopulateTableCell(tr6, janTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, febTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, marTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, aprTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, mayTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, junTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, julTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, augTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, sepTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, octTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, novTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, decTotal, Font, FontSize, tcp, naPP);
                        this.PopulateTableCell(tr6, totalTotal, Font, FontSize, tcp, naPP);

                        table.Append(tr6);
                    }
                }

                element.Append(table);
            }
        }

        /// <summary>
        /// Populates the GSMO Task Element table
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="taskRollup">task rollup data</param>
        /// <param name="TasksDateRange">date range of the tasks in the table</param>
        /// <param name="Format">tostring format for hours/costs</param>
        /// <param name="NumberFormatter">defines how numbers are formatted and displayed</param>
        /// <param name="Font">Font to use in the table</param>
        /// <param name="FontSize">Font size to use in the table</param>
        /// <param name="templateType">type of output template being exported</param>
        private void PopulateGSMOTaskElementRollup(SdtElement element, Dictionary<int, List<GSMOLaborRollupByDate>> taskRollup,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, ExcelReportTemplateType templateType)
        {
            if (element != null)
            {
                element.RemoveAllChildren();

                // Create the table
                Table table = this.CreateRollupTable();

                // Create the header row
                List<string> Headers;
                if (templateType == ExcelReportTemplateType.LMSI_STANDARD_LANDSCAPE)
                {
                    Headers = new List<string>() { "PROPRICER Resource Code", "Company", "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };
                }
                else
                {
                    Headers = new List<string>() { "RFP GSMO Labor Category", "Company", "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };
                }

                table.Append(this.CreateRollupHeaderRow(Headers, Font, FontSize, true));

                this.PopulateGSMOTaskElementRollupData(element, taskRollup, table, TasksDateRange, Format, NumberFormatter, Font, FontSize, false, JustificationValues.Center);
            }
        }

        /// <summary>
        /// Populates the GSMO Task Element table without the totals for each resource
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="taskRollup">task rollup data</param>
        /// <param name="TasksDateRange">date range of the tasks in the table</param>
        /// <param name="Format">tostring format for hours/costs</param>
        /// <param name="NumberFormatter">defines how numbers are formatted and displayed</param>
        /// <param name="Font">Font to use in the table</param>
        /// <param name="FontSize">Font size to use in the table</param>
        private void PopulateGSMOTaskElementRollupWithoutResTotals(SdtElement element, Dictionary<int, List<GSMOLaborRollupByDate>> taskRollup,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize)
        {
            if (element != null)
            {
                element.RemoveAllChildren();

                // Create the table
                Table table = this.CreateGSMORollupTable(TableLayoutValues.Fixed);

                // Create the header row
                string[] Headers = new string[] { "GSMO Labor Category", "Company", "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };

                TableRowProperties trp = new TableRowProperties(new TableHeader(), new CantSplit());
				TableRow tr = new TableRow();
                tr.Append(trp);
                if (Headers != null)
                {
                    foreach (string header in Headers)
                    {
                        TableCellProperties tcp;
                        if (header == "GSMO Labor Category")
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3528" }); // 3528 20ths of a point or 2.45 inches
                        }
                        else if (header == "Calendar Year")
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "1008" }); // 1008 20ths of a point or .7 inches
                        }
                        else
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "720" }); // 720 20ths of a point or 0.5 inches
                        }

                        RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });

                        ParagraphProperties pp = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new KeepNext() { Val = true }, new SpacingBetweenLines() { After = "0" });

                        this.PopulateTableCell(tr, header, Font, FontSize, tcp, pp, rp);
                    }
                }

                table.Append(tr);

                this.PopulateGSMOTaskElementRollupData(element, taskRollup, table, TasksDateRange, Format, NumberFormatter, Font, FontSize, true, JustificationValues.Right);
            }
        }

        /// <summary>
        /// Populates the SMORS Task Element table, with summaries for each resource
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="taskRollup">task rollup data</param>
        /// <param name="TasksDateRange">date range of the tasks in the table</param>
        /// <param name="Format">tostring format for hours/costs</param>
        /// <param name="NumberFormatter">defines how numbers are formatted and displayed</param>
        /// <param name="Font">Font to use in the table</param>
        /// <param name="FontSize">Font size to use in the table</param>
        /// <param name="includePerfOrg">if set to <c>true</c> [include perf org].</param>
        private void PopulateSMORSTaskElementRollup(SdtElement element, Dictionary<int, List<GSMOLaborRollupByDate>> taskRollup,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, bool includePerfOrg)
        {
            if (element != null)
            {
                element.RemoveAllChildren();

                // Create the table - can use same method as GSMO
                Table table = this.CreateGSMORollupTable(TableLayoutValues.Fixed);

                // Create the header row
                List<string> Headers = new List<string>() { "RFP SMORS Labor Category", "Calendar Year", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };
                if (includePerfOrg)
                {
                    // add the Perf Org header to the 3rd column
                    string[] perfOrg = new string[] { "Perf Org" };
                    Headers.InsertRange(2, perfOrg);
                }

                TableRowProperties trp = new TableRowProperties(new TableHeader(), new CantSplit());
				TableRow tr = new TableRow();
                tr.Append(trp);
                if (Headers != null)
                {
                    foreach (string header in Headers)
                    {
                        TableCellProperties tcp;
                        if (header == "RFP SMORS Labor Category")
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3571" }); // 3571 20ths of a point or 2.48 inches
                        }
                        else if (header == "Calendar Year")
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "1022" }); // 1022 20ths of a point or .71 inches
                        }
                        else if (includePerfOrg)
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "720" }); // 720 20ths of a point or 0.5 inches
                        }
                        else
                        {
                            tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "778" }); // 778 20ths of a point or 0.54 inches
                        }

                        ParagraphProperties pp = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new KeepNext() { Val = true }, new SpacingBetweenLines() { After = "0" });
                        RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                        this.PopulateTableCell(tr, header, Font, FontSize, tcp, pp, rp);
                    }
                }
                table.Append(tr);

                this.PopulateSMORSTaskElementRollupData(element, taskRollup, table, TasksDateRange, Format, NumberFormatter, Font, FontSize, includePerfOrg);
            }
        }

        /// <summary>
        /// Populates the data of the GSMO rollup table
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="taskRollup"> task rollup data</param>
        /// <param name="table">rollup table</param>
        /// <param name="TasksDateRange">date range of tasks in table</param>
        /// <param name="Format">tostring format for hours/costs</param>
        /// <param name="NumberFormatter">defines how numbers are formatted and displayed</param>
        /// <param name="Font">Font to use in the table</param>
        /// <param name="FontSize">Font size to use in the table</param>
        /// <param name="removeResourceTotals">bool that determines if resource tables should be removed</param>
        /// <param name="numberAlignment">Horizontal text alignment for number values in the table</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateGSMOTaskElementRollupData(SdtElement element, Dictionary<int, List<GSMOLaborRollupByDate>> taskRollup, Table table,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, bool removeResourceTotals, EnumValue<JustificationValues> numberAlignment)
        {
            if (!taskRollup.Keys.Any())
            {
                table.Append(this.CreateTableBlankRow(16));
            }

            TableVerticalAlignmentValues vertAlign = TableVerticalAlignmentValues.Center;

            ParagraphProperties naPP = new ParagraphProperties(new Justification() { Val = numberAlignment }, new SpacingBetweenLines() { After = "0" });
            ParagraphProperties rightPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Right }, new SpacingBetweenLines() { After = "0" });
            ParagraphProperties centerPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new SpacingBetweenLines() { After = "0" });

            // Create the year rows for each resource
            foreach (int item in taskRollup.Keys)
            {
                foreach (GSMOLaborRollupByDate item2 in taskRollup[item])
                {
					TableRow tr2 = new TableRow();
                    TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                    // populate the cells
                    this.PopulateTableCell(tr2, item2.LaborType, Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.Company, Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.Year.ToString(), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.January.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.February.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.March.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.April.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.May.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.June.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.July.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.August.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.September.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.October.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.November.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    this.PopulateTableCell(tr2, item2.December.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = "$";
                    }
                    this.PopulateTableCell(tr2, item2.Total.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, naPP);
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = string.Empty;
                    }

                    table.Append(tr2);
                }

                if (!removeResourceTotals)
                {
					TableRow tr3 = new TableRow();

                    // create resource total field
                    TableCellProperties totalLabelCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                        new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }));
                    RunProperties rp1 = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    this.PopulateTableCell(tr3, "Total", Font, FontSize, totalLabelCellProperties, rightPP, rp1, new GridSpan() { Val = 15 });

                    // and print the total value
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = "$";
                    }
                    TableCellProperties totalValueCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                        new TableCellBorders(new BottomBorder() { Val = BorderValues.Single, Color = "auto", Size = (UInt32Value)4U, Space = (UInt32Value)0U }));
                    string totalValue = taskRollup[item].Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                    this.PopulateTableCell(tr3, totalValue, Font, FontSize, totalValueCellProperties, naPP);
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = string.Empty;
                    }

                    table.Append(tr3);
                }

				// create blank separator row between resources
				TableRow tr4 = new TableRow();
                TableCellProperties blankCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new TableCellBorders(new TopBorder() { Val = BorderValues.Nil }));
				RunProperties blankCellRP = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                this.PopulateTableCell(tr4, string.Empty, Font, FontSize, blankCellProperties, centerPP, blankCellRP, new GridSpan() { Val = 16 }); // old gridspan value 15

                table.Append(tr4);
            }

            if (taskRollup.Keys.Any())
            {
                // Summary
                if (TasksDateRange.StartDate.HasValue && TasksDateRange.EndDate.HasValue)
                {
                    List<GSMOLaborRollupByDate> LaborValues = taskRollup.Values.SelectMany(x => x).ToList();

                    for (int i = TasksDateRange.StartDate.Value.Year; i <= TasksDateRange.EndDate.Value.Year; i++)
                    {
                        TableRow tr5 = new TableRow();

                        // create shaded summary cell
                        TableCellProperties summaryCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                            new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                        RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                        this.PopulateTableCell(tr5, "Summary", Font, FontSize, summaryCellProperties, centerPP, rp);

                        // blank cell
                        TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
                        this.PopulateTableCell(tr5, string.Empty, Font, FontSize, cellProperties, centerPP);

                        // get the summary value strings
                        string janSum = LaborValues.Where(x => x.Year == i).Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                        string febSum = LaborValues.Where(x => x.Year == i).Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                        string marSum = LaborValues.Where(x => x.Year == i).Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                        string aprSum = LaborValues.Where(x => x.Year == i).Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                        string maySum = LaborValues.Where(x => x.Year == i).Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                        string junSum = LaborValues.Where(x => x.Year == i).Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                        string julSum = LaborValues.Where(x => x.Year == i).Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                        string augSum = LaborValues.Where(x => x.Year == i).Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                        string sepSum = LaborValues.Where(x => x.Year == i).Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                        string octSum = LaborValues.Where(x => x.Year == i).Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                        string novSum = LaborValues.Where(x => x.Year == i).Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                        string decSum = LaborValues.Where(x => x.Year == i).Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = "$";
                        }
                        string total = LaborValues.Where(x => x.Year == i).Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                        if (NumberFormatter != null)
                        {
                            NumberFormatter.CurrencySymbol = string.Empty;
                        }

                        // populate the cells
                        this.PopulateTableCell(tr5, i.ToString(), Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, janSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, febSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, marSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, aprSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, maySum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, junSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, julSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, augSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, sepSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, octSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, novSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, decSum, Font, FontSize, cellProperties, naPP);
                        this.PopulateTableCell(tr5, total, Font, FontSize, cellProperties, naPP);

                        table.Append(tr5);
                    }

                    // Create the totals row
                    TableRow tr6 = new TableRow();

                    // create shaded summary cell
                    TableCellProperties totalSummaryCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                        new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                    RunProperties rp1 = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    this.PopulateTableCell(tr6, "Summary", Font, FontSize, totalSummaryCellProperties, centerPP, rp1);

                    // blank cell
                    TableCellProperties totalCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
                    this.PopulateTableCell(tr6, string.Empty, Font, FontSize, totalCellProperties, centerPP);

                    // total label
                    this.PopulateTableCell(tr6, "Total", Font, FontSize, totalCellProperties, centerPP, rp1);

                    // get the values
                    string janTotal = LaborValues.Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                    string febTotal = LaborValues.Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                    string marTotal = LaborValues.Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                    string aprTotal = LaborValues.Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                    string mayTotal = LaborValues.Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                    string junTotal = LaborValues.Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                    string julTotal = LaborValues.Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                    string augTotal = LaborValues.Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                    string sepTotal = LaborValues.Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                    string octTotal = LaborValues.Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                    string novTotal = LaborValues.Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                    string decTotal = LaborValues.Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = "$";
                    }
                    string totalTotal = LaborValues.Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                    if (NumberFormatter != null)
                    {
                        NumberFormatter.CurrencySymbol = string.Empty;
                    }

                    // populate cells with the above values
                    this.PopulateTableCell(tr6, janTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, febTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, marTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, aprTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, mayTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, junTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, julTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, augTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, sepTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, octTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, novTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, decTotal, Font, FontSize, totalCellProperties, naPP);
                    this.PopulateTableCell(tr6, totalTotal, Font, FontSize, totalCellProperties, naPP);

                    table.Append(tr6);
                }
            }

            element.Append(table);
        }

        /// <summary>
        /// Populates the data of the SMORS rollup table
        /// </summary>
        /// <param name="element">table element</param>
        /// <param name="taskRollup"> task rollup data</param>
        /// <param name="table">rollup table</param>
        /// <param name="TasksDateRange">date range of tasks in table</param>
        /// <param name="Format">tostring format for hours/costs</param>
        /// <param name="NumberFormatter">defines how numbers are formatted and displayed</param>
        /// <param name="Font">Font to use in the table</param>
        /// <param name="FontSize">Font size to use in the table</param>
        /// <param name="includesPerfOrg">Bool to determine if the perf org column is included</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void PopulateSMORSTaskElementRollupData(SdtElement element, Dictionary<int, List<GSMOLaborRollupByDate>> taskRollup, Table table,
            DateRange TasksDateRange, string Format, NumberFormatInfo NumberFormatter, string Font, string FontSize, bool includesPerfOrg)
        {
            int numColumns = (includesPerfOrg) ? 16 : 15;
            if (!taskRollup.Keys.Any())
            {
                table.Append(this.CreateTableBlankRow(numColumns));
            }

            TableVerticalAlignmentValues vertAlign = TableVerticalAlignmentValues.Center;
            ParagraphProperties centerPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Center }, new SpacingBetweenLines() { After = "0" });
            ParagraphProperties rightPP = new ParagraphProperties(new Justification() { Val = JustificationValues.Right }, new SpacingBetweenLines() { After = "0" });

            // Create the year rows for each resource
            foreach (int item in taskRollup.Keys)
            {
                foreach (GSMOLaborRollupByDate item2 in taskRollup[item])
                {
					TableRow tr2 = new TableRow();
                    TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                    // populate the cells
                    this.PopulateTableCell(tr2, item2.LaborType, Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.Year.ToString(), Font, FontSize, cellProperties, centerPP);
                    if (includesPerfOrg)
                    {
                        this.PopulateTableCell(tr2, item2.PerfOrg, Font, FontSize, cellProperties, centerPP);
                    }

                    this.PopulateTableCell(tr2, item2.January.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.February.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.March.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.April.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.May.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.June.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.July.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.August.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.September.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.October.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.November.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.December.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);
                    this.PopulateTableCell(tr2, item2.Total.ToString(Format, NumberFormatter), Font, FontSize, cellProperties, centerPP);

                    table.Append(tr2);
                }

				TableRow tr3 = new TableRow();

                // create resource total field
                TableCellProperties totalLabelCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new TableCellBorders(new BottomBorder() { Val = BorderValues.Nil }));
                RunProperties rp1 = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                this.PopulateTableCell(tr3, "Total", Font, FontSize, totalLabelCellProperties, rightPP, rp1, new GridSpan() { Val = numColumns - 1 });

                // and print the total value
                if (NumberFormatter != null)
                {
                    NumberFormatter.CurrencySymbol = "$";
                }
                TableCellProperties totalValueCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct },
                    new TableCellBorders(new BottomBorder() { Val = BorderValues.Single, Color = "auto", Size = (UInt32Value)4U, Space = (UInt32Value)0U }));
                string totalValue = taskRollup[item].Select(x => x.Total).Sum().ToString(Format, NumberFormatter);
                this.PopulateTableCell(tr3, totalValue, Font, FontSize, totalValueCellProperties, centerPP);
                if (NumberFormatter != null)
                {
                    NumberFormatter.CurrencySymbol = string.Empty;
                }

                table.Append(tr3);

				// create blank separator row between resources
				TableRow tr4 = new TableRow();
                TableCellProperties blankCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new TableCellBorders(new TopBorder() { Val = BorderValues.Nil }));
				RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                this.PopulateTableCell(tr4, string.Empty, Font, FontSize, blankCellProperties, centerPP, rp, new GridSpan() { Val = numColumns });

                table.Append(tr4);
            }

            if (taskRollup.Keys.Any())
            {
                // Summary
                if (TasksDateRange.StartDate.HasValue && TasksDateRange.EndDate.HasValue)
                {
                    List<GSMOLaborRollupByDate> LaborValues = taskRollup.Values.SelectMany(x => x).ToList();

                    for (int i = TasksDateRange.StartDate.Value.Year; i <= TasksDateRange.EndDate.Value.Year; i++)
                    {
                        TableRow tr5 = new TableRow();

                        // create shaded summary cell
                        TableCellProperties summaryCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                            new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                        RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                        this.PopulateTableCell(tr5, "Summary", Font, FontSize, summaryCellProperties, centerPP, rp);

                        // get the summary value strings
                        string janSum = LaborValues.Where(x => x.Year == i).Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                        string febSum = LaborValues.Where(x => x.Year == i).Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                        string marSum = LaborValues.Where(x => x.Year == i).Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                        string aprSum = LaborValues.Where(x => x.Year == i).Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                        string maySum = LaborValues.Where(x => x.Year == i).Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                        string junSum = LaborValues.Where(x => x.Year == i).Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                        string julSum = LaborValues.Where(x => x.Year == i).Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                        string augSum = LaborValues.Where(x => x.Year == i).Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                        string sepSum = LaborValues.Where(x => x.Year == i).Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                        string octSum = LaborValues.Where(x => x.Year == i).Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                        string novSum = LaborValues.Where(x => x.Year == i).Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                        string decSum = LaborValues.Where(x => x.Year == i).Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                        string total = LaborValues.Where(x => x.Year == i).Select(x => x.Total).Sum().ToString(Format, NumberFormatter);

                        TableCellProperties cellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                        // populate the cells
                        this.PopulateTableCell(tr5, i.ToString(), Font, FontSize, cellProperties, centerPP);
                        if (includesPerfOrg)
                        {
                            this.PopulateTableCell(tr5, string.Empty, Font, FontSize, cellProperties, centerPP); // blank in summary rows
                        }

                        this.PopulateTableCell(tr5, janSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, febSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, marSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, aprSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, maySum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, junSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, julSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, augSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, sepSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, octSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, novSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, decSum, Font, FontSize, cellProperties, centerPP);
                        this.PopulateTableCell(tr5, total, Font, FontSize, cellProperties, centerPP);

                        table.Append(tr5);
                    }

                    // Create the totals row
                    TableRow tr6 = new TableRow();

                    // create shaded summary cell
                    TableCellProperties totalSummaryCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign },
                        new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear });
                    RunProperties rp1 = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
                    this.PopulateTableCell(tr6, "Summary", Font, FontSize, totalSummaryCellProperties, centerPP, rp1);

                    // total label
                    TableCellProperties totalCellProperties = new TableCellProperties(new TableCellVerticalAlignment() { Val = vertAlign }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
                    this.PopulateTableCell(tr6, "Total", Font, FontSize, totalCellProperties, centerPP, rp1);

                    // get the values
                    string janTotal = LaborValues.Select(x => x.January).Sum().ToString(Format, NumberFormatter);
                    string febTotal = LaborValues.Select(x => x.February).Sum().ToString(Format, NumberFormatter);
                    string marTotal = LaborValues.Select(x => x.March).Sum().ToString(Format, NumberFormatter);
                    string aprTotal = LaborValues.Select(x => x.April).Sum().ToString(Format, NumberFormatter);
                    string mayTotal = LaborValues.Select(x => x.May).Sum().ToString(Format, NumberFormatter);
                    string junTotal = LaborValues.Select(x => x.June).Sum().ToString(Format, NumberFormatter);
                    string julTotal = LaborValues.Select(x => x.July).Sum().ToString(Format, NumberFormatter);
                    string augTotal = LaborValues.Select(x => x.August).Sum().ToString(Format, NumberFormatter);
                    string sepTotal = LaborValues.Select(x => x.September).Sum().ToString(Format, NumberFormatter);
                    string octTotal = LaborValues.Select(x => x.October).Sum().ToString(Format, NumberFormatter);
                    string novTotal = LaborValues.Select(x => x.November).Sum().ToString(Format, NumberFormatter);
                    string decTotal = LaborValues.Select(x => x.December).Sum().ToString(Format, NumberFormatter);
                    string totalTotal = LaborValues.Select(x => x.Total).Sum().ToString(Format, NumberFormatter);

                    // populate the cells with the above values
                    if (includesPerfOrg)
                    {
                        this.PopulateTableCell(tr6, string.Empty, Font, FontSize, totalCellProperties, centerPP); // blank in total row
                    }

                    this.PopulateTableCell(tr6, janTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, febTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, marTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, aprTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, mayTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, junTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, julTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, augTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, sepTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, octTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, novTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, decTotal, Font, FontSize, totalCellProperties, centerPP);
                    this.PopulateTableCell(tr6, totalTotal, Font, FontSize, totalCellProperties, centerPP);

                    table.Append(tr6);
                }
            }

            element.Append(table);
        }

        /// <summary>
        /// Populates a cell of a rollup table
        /// </summary>
        /// <param name="tableRow">The row containing the cell</param>
        /// <param name="text">The text to populate the cell with</param>
        /// <param name="font">Font to be used</param>
        /// <param name="fontSize">Size of the font</param>
        /// <param name="cellProperties">Properties to apply to the table cell</param>
        /// <param name="paragraphProperties">Properties to apply to the paragraph (main text container)</param>
        /// <param name="runProperties">Properties to apply to the run (text container within the paragraph) - optional</param>
        /// <param name="gridSpan">How many cells this cell should span across - optional, default is one when not set</param>
        protected void PopulateTableCell(TableRow tableRow, string text, string font, string fontSize, TableCellProperties cellProperties, ParagraphProperties paragraphProperties, RunProperties runProperties = null, GridSpan gridSpan = null)
        {
            if (tableRow == null)
            {
                throw new ArgumentNullException(nameof(tableRow));
            }
            if (cellProperties == null)
            {
                throw new ArgumentNullException(nameof(cellProperties));
            }
            if (paragraphProperties == null)
            {
                throw new ArgumentNullException(nameof(paragraphProperties));
            }

            TableCell tc = new TableCell();
            tc.PrependChild(cellProperties.CloneNode(true));
            if (gridSpan != null)
            {
                tc.Append(gridSpan);
            }

            ParagraphProperties pp = new ParagraphProperties(paragraphProperties.CloneNode(true));
            RunProperties rp;
            if (runProperties != null)
            {
                rp = new RunProperties(runProperties.CloneNode(true));
            }
            else
            {
                rp = new RunProperties();
            }

            RunFonts runFonts = new RunFonts() { Ascii = font, HighAnsi = font, ComplexScript = font };
            FontSize fs = new FontSize() { Val = fontSize };
            FontSizeComplexScript fontSizeComplexScript2 = new FontSizeComplexScript() { Val = fontSize };
            rp.Append(runFonts);
            rp.Append(fs);
            rp.Append(fontSizeComplexScript2);
            Run run = new Run();
            run.Append(rp);
            if (text != null)
            {
                Text text1 = new Text();
                text1.Text = text;
                run.Append(text1);
            }

            Paragraph p = new Paragraph(run);
            p.PrependChild<ParagraphProperties>(pp);
            tc.Append(p);
            tableRow.Append(tc);
        }

        private TableRow CreateTableBlankRow(int NumOfColumns)
        {
			TableRow tr2 = new TableRow();

            for (int i = 0; i < NumOfColumns; i++)
            {
				TableCell tc = new TableCell();
                tc.PrependChild(new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct }));
                ParagraphProperties pp = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                Paragraph p = new Paragraph(new Run(new Text(string.Empty)));
                p.PrependChild<ParagraphProperties>(pp);
                tc.Append(p);
                tr2.Append(tc);
            }

            return tr2;
        }

        /// <summary>
        /// Generates a new task variable detail table object with the standard formatting
        /// </summary>
        /// <returns>the generated table</returns>
        private Table CreateTaskVariableTable()
        {
            Table table = new Table();
            TableProperties props = new TableProperties(
                new TableBorders(
                    new TopBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new BottomBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new LeftBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new RightBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideHorizontalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    },
                    new InsideVerticalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 4
                    }), new TableWidth() { Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa), Width = "9940" },
                new TableLook() { Val = "04A0", FirstRow = true, LastRow = false, FirstColumn = true, LastColumn = false, NoHorizontalBand = false, NoVerticalBand = true });
            table.AppendChild<TableProperties>(props);

            return table;
        }

        /// <summary>
        /// Builds and appends the table header to the passed in table
        /// </summary>
        /// <param name="headers">The header strings added to the header. The count determines the number of cells in the table</param>
        /// <param name="table">The table object that the generated headed is appended to</param>
        /// <param name="inFont">The font.</param>
        /// <param name="inFontSize">Size of the font.</param>
        private void AppendTaskVariableTableHeader(List<string> headers, Table table, string inFont, string inFontSize)
        {
			TableRow tr = new TableRow();
            TableRowProperties tableRowProperties = new TableRowProperties();
            CantSplit cantSplit = new CantSplit();
            tableRowProperties.Append(cantSplit);
            TableHeader tableHeader = new TableHeader();
            tableRowProperties.Append(tableHeader);
            tr.Append(tableRowProperties);

            TableCellProperties tcp = new TableCellProperties(new Shading() { Color = "auto", Fill = "BFBFBF", Val = ShadingPatternValues.Clear },
                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
            RunProperties rp = new RunProperties(new Bold() { Val = OnOffValue.FromBoolean(true) });
            ParagraphProperties pp = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

            this.PopulateTableCell(tr, "Variable", inFont, inFontSize, tcp, pp, rp, new GridSpan() { Val = 2 });
            this.PopulateTableCell(tr, "Dependents", inFont, inFontSize, tcp, pp, rp, new GridSpan() { Val = 3 });

            table.Append(tr);

            TableRow tr2 = this.CreateRollupHeaderRow(headers, inFont, inFontSize, false);
            table.Append(tr2);
        }

        /// <summary>
        /// Builds and populates the MOQ variable details table content
        /// </summary>
        /// <param name="mvs">the model views containing the data to populate cells</param>
        /// <param name="table">The table object the generated content rows are added to</param>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        private void PopulateTaskVariableTable(Collection<BOEExportTaskElementMOQVariableModelView> mvs, Table table)
        {
            string Font = "Times New Roman";
            string FontSize = "24";

            foreach (BOEExportTaskElementMOQVariableModelView item in mvs)
            {
                bool initialpass = true;

                foreach (BOEExportTaskElementMOQVariableBOEModelView item2 in item.ReferencedBOEs)
                {
					TableRow tr = new TableRow();

                    // Populate Variable Cells
                    TableCellProperties VariableTCP;
                    if (initialpass && item.ReferencedBOEs.Count == 1)
                    {
                        VariableTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });
                    }
                    else if (initialpass && item.ReferencedBOEs.Count > 1)
                    {
                        VariableTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new VerticalMerge() { Val = MergedCellValues.Restart });
                    }
                    else
                    {
                        VariableTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct }, new VerticalMerge() { Val = MergedCellValues.Continue });
                    }

                    ParagraphProperties pp = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                    string name = null;
                    if (initialpass)
                    {
                        name = item.OrdinaryVariableName;
                    }

                    string total = null;
                    if (initialpass)
                    {
                        total = item.Total.ToString(this.DefaultHoursFormat);
                    }

                    this.PopulateTableCell(tr, name, Font, FontSize, VariableTCP, pp);
                    this.PopulateTableCell(tr, total, Font, FontSize, VariableTCP, pp);

                    // Populate Dependents Cells
                    TableCellProperties DependentsTCP = new TableCellProperties(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }, new TableCellWidth() { Type = TableWidthUnitValues.Pct });

                    this.PopulateTableCell(tr, Utilities.FormatNumberTitleString(item2.WBSNumber, item2.WBSTitle, " - "), Font, FontSize, DependentsTCP, pp);
                    this.PopulateTableCell(tr, item2.ClinNumber, Font, FontSize, DependentsTCP, pp);
                    this.PopulateTableCell(tr, item2.Total.ToString(this.DefaultHoursFormat), Font, FontSize, DependentsTCP, pp);

                    table.Append(tr);
                    initialpass = false;
                }
            }
        }

        /// <summary>
        /// Take a task element and populate the task-specific items in the template with its
        /// data. A new task table row (landscape) or table (portrait) should already be created
        /// before this function is called. This function will then populate that empty row/table.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="taskContainer">The task container.</param>
        /// <param name="taskElement">The task element whose data will populate the template</param>
        /// <param name="ws">Full WS</param>
        /// <param name="mainPart">Main Document Part</param>
        /// <param name="wsHasMoqRteTemplate">Whether there are any MOQ RTE Templates</param>
        /// <param name="counters">The counters.</param>
        private void PopulateTaskElementContent(BOEExportInputs exportInputs, BOEExportTaskContainer taskContainer, BOEExportTaskElement taskElement, FullWorkspace ws,
            MainDocumentPart mainPart, bool wsHasMoqRteTemplate, ref ChunkCounter counters)
        {
            // Iterate over all SdtElements in the document
            foreach (SdtAlias alias in taskContainer.TaskContainer.Descendants<SdtAlias>().ToList())
            {
                // Get the title of this Alias
                string sdtTitle = alias.Val.Value;

                // Get the Element that encapsulates the current alias
                SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();

                // If the current element is not null, populate it with the appropriate data from the BOEExportModelView
                if (element != null && element != taskContainer.TaskContainer)
                {
                    if (sdtTitle == FieldName_TaskElementTitle)
                    {
                        SetElementText(element, taskElement.TaskTitle);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskElementDescription)
                    {
                        if (exportInputs.Workspace.IsProjectMapWorkspace)
                        {
                            SetElementText(element, taskElement.BOETaskDesc);
                        }
                        else
                        {
                            WordUtilities.SetElementTextWithHTML(mainPart, element, taskElement.BOETaskDesc, ref counters);
                        }

                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskDescriptionSSDS)
                    {
                        WordUtilities.SetElementTextWithHTML(mainPart, element, this.ReplaceParagraphTags(taskElement.BOETaskDesc), ref counters);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskStartDate)
                    {
                        string value = string.Empty;

                        if (taskElement.StartDate.HasValue)
                        {
                            value = taskElement.StartDate.Value.ToString("MM/yyyy");
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskEndDate)
                    {
                        string value = string.Empty;

                        if (taskElement.EndDate.HasValue)
                        {
                            value = taskElement.EndDate.Value.ToString("MM/yyyy");
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskSegregation)
                    {
                        string value = string.Empty;
                        if (taskElement.ExportFields.ContainsKey(FieldName_TaskSegregation))
                        {
                            value = taskElement.ExportFields[FieldName_TaskSegregation];
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_UID)
                    {
                        if (taskElement.IMS_ID != null && taskElement.IMS_ID.Length > 0)
                        {
                            SetElementText(element, taskElement.BOETaskID);
                            alias.RemoveIt();
                        }
                        else
                        {
                            element.Parent.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_TaskID)
                    {
                        if (taskElement.BOETaskID != null && taskElement.BOETaskID.Length > 0)
                        {
                            SetElementText(element, "Task #" + taskElement.BOETaskID + "\t");
                            alias.RemoveIt();
                        }
                        else
                        {
                            element.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_TaskIDNumber)
                    {
                        if (taskElement.BOETaskID != null && taskElement.BOETaskID.Length > 0)
                        {
                            SetElementText(element, taskElement.BOETaskID);
                            alias.RemoveIt();
                        }
                        else
                        {
                            element.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_GenBOETaskID)
                    {
                        if (taskElement.BOETaskElementID != null)
                        {
                            SetElementText(element, taskElement.BOETaskElementID.ToString());
                            alias.RemoveIt();
                        }
                        else
                        {
                            element.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_MOQEquation)
                    {
                        if (taskElement.ElementType == BOEExportTaskElementType.Labor)
                        {
                            SetElementText(element, this.GetMOQEquationToDisplay(taskElement, exportInputs, ws));
                            alias.RemoveIt();
                        }
                        else
                        {
                            element.Parent.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_MOQOriginalVarsTable)
                    {
                        if (taskElement.ElementType == BOEExportTaskElementType.Labor)
                        {
                            string useFontSize = "24";
                            string useFont = "Times New Roman";

                            element.RemoveAllChildren();

                            if (taskElement.MOQVariableModelViews.Any())
                            {
                                Table table = this.CreateTaskVariableTable();
                                string hoursLabel = FullObjectHelper.HoursLabel(exportInputs.Workspace);
                                List<string> Headers = new List<string>() { "Name", "Total", "WBS Number/Title", "CLIN", hoursLabel };
                                if (taskElement.ExportFields.ContainsKey(FieldName_ExportFormatId))
                                {
                                    string id = taskElement.ExportFields[FieldName_ExportFormatId];
                                    string gsmoTemplateId = ((int)ExcelReportTemplateType.LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES).ToString();
                                    useFontSize = (id == gsmoTemplateId) ? "22" : "24";
                                }

                                this.AppendTaskVariableTableHeader(Headers, table, useFont, useFontSize);
                                this.PopulateTaskVariableTable(taskElement.MOQVariableModelViews, table);
                                element.Append(table);
                            }

                            alias.RemoveIt();
                        }
                        else
                        {
                            element.Parent.RemoveIt();
                        }
                    }
                    else if (sdtTitle == FieldName_MOQEquationResult)
                    {
                        SetElementText(element, this.GetMOQTotal(taskElement, exportInputs, ws.TaskElements));
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_MethodOfQuoting || sdtTitle == FieldName_Rationale)
                    {
                        if (exportInputs.Workspace.UsingTemplateBOE && !wsHasMoqRteTemplate)
                        {
                            // remove this for Workspaces using Template BOE if they have no MOQ RTE templates
                            // call both remove row and element since it can be either depending on the template
                            // both methods already handle there not being a row/element, so both can be run safely without affecting the other
                            WordUtilities.RemoveTableRowWithTaggedElement(taskContainer.TaskContainer, sdtTitle);
                            WordUtilities.RemoveTaggedElement(taskContainer.TaskContainer, sdtTitle);
                        }
                        else
                        {
                            OpenXmlCompositeElement item;
                            if ((item = element.ChildElements.OfType<SdtContentBlock>().FirstOrDefault()) != null ||
                                (item = element.ChildElements.OfType<SdtContentRun>().FirstOrDefault()) != null)
                            {
                                if (taskElement.MOQText == null)
                                {
                                    taskElement.MOQText = "<p><br></p>"; // filler text that allows removal of place-holder text, otherwise "Method of Quoting" remains
                                }

                                if (exportInputs.Workspace.IsProjectMapWorkspace)
                                {
                                    SetElementText(item, taskElement.MOQText);
                                }
                                else
                                {
                                    WordUtilities.SetElementTextWithHTML(mainPart, item, taskElement.MOQText, ref counters);
                                }

                                if (wsHasMoqRteTemplate && exportInputs.Workspace.UsingTemplateBOE)
                                {
                                    // replace the label for RTE Templates in MOQ Types
                                    SdtElement moqLabelElement = WordUtilities.GetTaggedChildElement(taskContainer.TaskContainer, FieldName_MethodOfQuotingLabel);
                                    WordUtilities.SetElementText(moqLabelElement, "Additional MOQ Rationale: ");
                                }
                            }

                            alias.RemoveIt();
                        }
                    }
                    else if (sdtTitle == BOEExporterConstants.Container_MOQSelection)
                    {
                        if (exportInputs.Workspace.UsingTemplateBOE)
                        {
                            this.PopulateMOQTypeData(taskElement, new Collection<BoeCustomReportComponent>(), mainPart, element, false, exportInputs, ref counters);
                        }
                        else
                        {
                            // remove this for Workspaces not using Template BOE
                            WordUtilities.RemoveTaggedElement(taskContainer.TaskContainer, sdtTitle);
                        }
                    }
                    else if (sdtTitle == FieldName_MOQSSDS)
                    {
                        WordUtilities.SetElementTextWithHTML(mainPart, element, this.ReplaceParagraphTags(taskElement.MOQText), ref counters);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskCostCenter)
                    {
                        string value = string.Empty;
                        if (taskElement.ExportFields.ContainsKey(FieldName_TaskCostCenter))
                        {
                            value = taskElement.ExportFields[FieldName_TaskCostCenter];
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (taskElement.ExportFields.ContainsKey(sdtTitle))
                    {
                        SetElementText(element, taskElement.ExportFields[sdtTitle]);
                        alias.RemoveIt();
                    }
                }
            }

            if (taskElement.taskElementLabors.Any())
            {
                foreach (BOEExportTaskElementLabor taskElementType in taskElement.taskElementLabors)
                {
                    if (taskContainer.TaskTypeRow != null)
                    {
                        this.PopulateTaskTypeContent(this.AddAnotherTaskType(taskContainer), taskElementType);
                    }
                }
            }

            taskContainer.TaskTypeRow.RemoveIt();
        }


        /// <summary>
        /// Populates content of the resource container
        /// </summary>
        /// <param name="resourceContainer">Resource Container to be populated</param>
        /// <param name="resource">Resource info to populate container with</param>
        private void PopulateResourceContent(BOEExportTaskContainer resourceContainer, BOEExportTaskElementLabor resource)
        {
            // Iterate over all SdtElements in the document
            foreach (SdtAlias alias in resourceContainer.TaskContainer.Descendants<SdtAlias>().ToList())
            {
                // Get the title of this Alias
                string sdtTitle = alias.Val.Value;

				// Get the Element that encapsulates the current alias
				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();

                // If the current element is not null, populate it with the appropriate data from the BOEExportModelView
                if (element != null && element != resourceContainer.TaskContainer)
                {
                    if (sdtTitle == FieldName_ResourceStartDate)
                    {
                        string value = string.Empty;

                        if (resource.StartDate.HasValue)
                        {
                            value = resource.StartDate.Value.ToString("MM/yyyy");
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_ResourceEndDate)
                    {
                        string value = string.Empty;

                        if (resource.EndDate.HasValue)
                        {
                            value = resource.EndDate.Value.ToString("MM/yyyy");
                        }

                        SetElementText(element, value);
                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_TaskTypeDescription)
                    {
                        if (resource.ExportFields.ContainsKey(FieldName_TaskTypeDescription))
                        {
                            SetElementText(element, resource.ExportFields[FieldName_TaskTypeDescription]);
                        }

                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_PerfOrg)
                    {
                        if (resource.ExportFields.ContainsKey(FieldName_PerfOrg))
                        {
                            SetElementText(element, resource.ExportFields[FieldName_PerfOrg]);
                        }

                        alias.RemoveIt();
                    }
                    else if (sdtTitle == FieldName_GenBOEResourceID)
                    {
                        if (resource.ExportFields.ContainsKey(FieldName_GenBOEResourceID))
                        {
                            SetElementText(element, resource.ExportFields[FieldName_GenBOEResourceID]);
                        }

                        alias.RemoveIt();
                    }
                }
            }
        }

        protected void PopulateTaskTypeContent(OpenXmlElement taskTypeRow, BOEExportTaskElementLabor taskElementType)
        {
            if (taskTypeRow == null)
            {
                throw new ArgumentNullException(nameof(taskTypeRow));
            }

            // Iterate over all SdtElements in the document
            foreach (SdtAlias alias in taskTypeRow.Descendants<SdtAlias>().ToList())
            {
                // Get the title of this Alias
                string sdtTitle = alias.Val.Value;

				// Get the Element that encapsulates the current alias
				SdtElement element = alias.Ancestors<SdtElement>().FirstOrDefault();

                // If the current element is not null, populate it with the appropriate data from the BOEExportModelView
                if (element != null)
                {
                    if (taskElementType != null && taskElementType.ExportFields.ContainsKey(sdtTitle))
                    {
                        SetElementText(element, taskElementType.ExportFields[sdtTitle]);
                        alias.RemoveIt();
                    }
                    else
                    {
                        // Special cases to remove paragraphs for certain empty elements
                        if (sdtTitle == FieldName_TaskTypeTitle)
                        {
                            // Remove the element's parent
                            element.Parent.RemoveIt();
                        }
                        else
                        {
                            if (element.Parent.GetType() != typeof(Paragraph))
                            {
                                element.Parent.AppendChild(this.CreateNewTextParagraph(null, string.Empty));
                            }

                            element.RemoveIt();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds a special element in the landscape template that is used to mark the location just before the
        /// repeatable BOE details table.
        /// </summary>
        /// <param name="document">The Word document to search</param>
        /// <returns>Table element representing the spot just before the repeatable BOE details table row</returns>
        private SdtElement FetchRepeatableBOEElement(WordprocessingDocument document)
        {
            SdtElement toReturn = null;

			// Get all SdtElements (tags) that have Aliases (tag names) attached to them
			List<SdtElement> sdtElementsWithAliases = document.MainDocumentPart.Document.Descendants<SdtElement>().Where(s => s.Descendants<SdtAlias>().Any()).ToList();

            // If there are SdtElements with names
            if (sdtElementsWithAliases.Any())
            {
				// Attempt to find the SdtElement with a special name for the landscape template
				SdtElement finder = sdtElementsWithAliases.FirstOrDefault(s => s.Descendants<SdtAlias>().FirstOrDefault().Val.Value == FieldName_BOETable);

                // If the special element was found, return it
                if (finder != null)
                {
                    toReturn = finder;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Creates a new Paragraph element with the given text elements
        /// </summary>
        /// <param name="inRunProperties">The run properties.</param>
        /// <param name="text">The text elements to include in the new text run. Multiple elements are separated with carriage returns.</param>
        /// <returns>
        /// A new Paragraph element containing the given text
        /// </returns>
        private Paragraph CreateNewTextParagraph(RunProperties inRunProperties, params string[] text)
        {
            Paragraph toReturn = new Paragraph();
            toReturn.AppendChild(this.CreateNewTextRun(inRunProperties, text));
            return toReturn;
        }

        /// <summary>
        /// Creates a single run of text within a paragraph element
        /// </summary>
        /// <param name="inRunProperties">The run properties.</param>
        /// <param name="text">The text elements to include in the new text run. Multiple elements are separated with carriage returns.</param>
        /// <returns>
        /// A new Run element containing the given text
        /// </returns>
        private Run CreateNewTextRun(RunProperties inRunProperties, params string[] text)
        {
            Run toReturn = new Run();

            for (int ndx = 0; ndx < text.Length; ndx++)
            {
                // the task title is the only field in the document that needs to be bolded
                if (inRunProperties != null)
                {
                    // Clone the run properties to avoid errors from using properties elements already in the document
                    inRunProperties = inRunProperties.CloneNode(true) as RunProperties;

                    // Append the given run properties to the text run
                    toReturn.AppendChild(inRunProperties);
                }

                Text t = new Text(text[ndx]);
                toReturn.AppendChild(t);

                if (ndx < text.Length - 1)
                {
                    toReturn.AppendChild(new Break());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Sets the text of a run within a Content Element
        /// </summary>
        /// <param name="inElement">The element to set text on</param>
        /// <param name="inText">The text to set</param>
        private static void SetElementText(OpenXmlElement inElement, params string[] inText)
        {
            if (inText != null)
            {
                inText = (from text in inText
                    where text != null
                    select text).ToArray();
            }

            // If the passed array of strings is null, set it to an empty string
            if (inText == null || inText.Length == 0)
            {
                inText = new string[] { string.Empty };
            }

            // Break each text value at the line breaks (\n character)
            inText = (from text in inText
                where text != null
                from brokenText in text.Split('\n')
                select brokenText).ToArray();

			// Get the first text run in the element
			Run textRun = inElement.Descendants<Run>().FirstOrDefault();

            // If a text run was found
            if (textRun != null)
            {
                // Clone the original text run so we can make copies of it (for multiple input strings)
                OpenXmlElement textRunClone = textRun.CloneNode(true);

                // Get the parent of the run
                OpenXmlElement runParent = textRun.Parent;

                // Iterate through the given input strings and append each one as a text Run
                for (int ndx = 0; ndx < inText.Length; ndx++)
                {
                    // Get the text within the Run
                    Text textElement = textRun.Descendants<Text>().FirstOrDefault();

                    if (textElement == null)
                    {
                        break;  // if there is no Text node, then we can't write ANYTHING, so exit the method
                    }
                    else
                    {
                        // Add the input string ...
                        textElement.Text = inText[ndx];

                        // ... followed by a line-break
                        if (ndx < inText.Length - 1)  // (but not after the last one)
                        {
                            textRun.AppendChild(new Break());  // <br>
                        }

                        // Add the Run to the document
                        if (ndx > 0)  // (the original one is already there, so skip it)
                        {
                            runParent.AppendChild(textRun);
                        }

                        // Create a copy for the next input string
                        textRun = textRunClone.CloneNode(true) as Run;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all of the Task Containers available for each BOEExportTaskType
        /// </summary>
        /// <param name="boeElement">The BOE containing the tasks that will be retrieved</param>
        /// <param name="inTaskContainers">The task containers.</param>
        private void FetchContainerElements(OpenXmlElement boeElement, Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inTaskContainers)
        {
			// Get all SdtElements (tags) that have Aliases (tag names) attached to them
			List<SdtAlias> sdtAliases = boeElement.Descendants<SdtAlias>().ToList();

            // If there are SdtElements with names
            if (sdtAliases.Any())
            {
                foreach (BOEExportTaskElementType boeExportTaskElementType in Enum.GetValues(typeof(BOEExportTaskElementType)).Cast<BOEExportTaskElementType>())
                {
                    this.FetchSDTContainerElement(sdtAliases, boeExportTaskElementType, inTaskContainers);
                    this.FetchRowContainerElement(sdtAliases, boeExportTaskElementType, boeElement, inTaskContainers);
                }
            }
        }

        /// <summary>
        /// Gets elements for the resource containers
        /// </summary>
        /// <param name="taskElement">task element containing elements for containers</param>
        /// <param name="inResourceContainers">Containers to fetch elements for</param>
        private void FetchResourceContainerElements(OpenXmlElement taskElement, Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inResourceContainers)
        {
			// Get all SdtElements (tags) that have Aliases (tag names) attached to them
			List<SdtAlias> sdtAliases = taskElement.Descendants<SdtAlias>().ToList();

            // If there are SdtElements with names
            if (sdtAliases.Any())
            {
                foreach (BOEExportTaskElementType boeExportTaskElementType in ExtensionMethods.GetEnumValues<BOEExportTaskElementType>())
                {
                    SdtElement taskContainer = null;

					// Attempt to find the SdtAlias with a special name for the current task type
					SdtAlias alias = sdtAliases.LastOrDefault(s => s.Val.Value.Equals(FieldName_ResourceContainer + "-" + ExtensionMethods.GetName(boeExportTaskElementType)));

                    // If the special alias was found, return its parent SDT Element
                    if (alias != null)
                    {
                        // Get the parent SdtElement
                        taskContainer = alias.Ancestors<SdtElement>().FirstOrDefault();

                        if (taskContainer != null)
                        {
							// Add the element to the collection of task types
							TableRow taskTypeRow = this.FetchTaskTypeRowFinder(taskContainer, boeExportTaskElementType);
                            BOEExportTaskContainer newTaskContainer;
                            if (taskTypeRow == null)
                            {
                                newTaskContainer = new BOEExportTaskContainer(taskContainer);
                            }
                            else
                            {
                                newTaskContainer = new BOEExportTaskContainer(taskContainer, taskTypeRow);
                            }
                            inResourceContainers.Add(boeExportTaskElementType, newTaskContainer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Looks for task container elements that are encapsulated by SDTElements and which contain task type rows
        /// as child elements. This is the typical pattern for portrait templates.
        /// </summary>
        /// <param name="sdtAliases">The SDT Aliases in the current BOE</param>
        /// <param name="boeExportTaskElementType">The type of task being fetched</param>
        /// <param name="inTaskContainers">The task containers.</param>
        private void FetchSDTContainerElement(ICollection<SdtAlias> sdtAliases, BOEExportTaskElementType boeExportTaskElementType, Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inTaskContainers)
        {
			// Attempt to find the SdtAlias with a special name for the current task type
			SdtAlias alias = sdtAliases.LastOrDefault(s => s.Val.Value.Equals("BOE:TaskContainer-" + Enum.GetName(typeof(BOEExportTaskElementType), boeExportTaskElementType), StringComparison.CurrentCultureIgnoreCase));

            // If the special alias was found, return its parent SDT Element
            if (alias != null)
            {
                // Get the parent SdtElement
                SdtElement taskContainer = alias.Ancestors<SdtElement>().FirstOrDefault();

                if (taskContainer != null)
                {
					// Add the element to the collection of task types
					TableRow taskTypeRow = this.FetchTaskTypeRowFinder(taskContainer, boeExportTaskElementType);
                    BOEExportTaskContainer newTaskContainer;
                    if (taskTypeRow == null)
                    {
                        newTaskContainer = new BOEExportTaskContainer(taskContainer);
                    }
                    else
                    {
                        newTaskContainer = new BOEExportTaskContainer(taskContainer, taskTypeRow);
                    }
                    inTaskContainers.Add(boeExportTaskElementType, newTaskContainer);
                }
            }
        }

        /// <summary>
        /// Looks for task container elements that are rows within a table and whose cooresponding task type rows are
        /// sibling elements. This is the typical pattern for landscape templates.
        /// </summary>
        /// <param name="sdtAliases">The SDT Aliases in the current BOE</param>
        /// <param name="boeExportTaskElementType">The type of task being fetched</param>
        /// <param name="boeElement">The container BOE will be used to look for this type's task type row, since it will be a sibling
        /// of the task row</param>
        /// <param name="inTaskContainers">The task containers.</param>
        private void FetchRowContainerElement(ICollection<SdtAlias> sdtAliases, BOEExportTaskElementType boeExportTaskElementType, OpenXmlElement boeElement, Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inTaskContainers)
        {


			// Attempt to find the SdtAlias with a special name for the current task type
			SdtAlias alias = sdtAliases.LastOrDefault(s => s.Val.Value.Equals("BOE:TaskRow-" + Enum.GetName(typeof(BOEExportTaskElementType), boeExportTaskElementType), StringComparison.CurrentCultureIgnoreCase));

            // If the special alias was found, return its parent SDT Element
            if (alias != null)
            {
                // Get the parent SdtElement
                TableRow taskContainer = alias.Ancestors<TableRow>().FirstOrDefault();

                if (taskContainer != null)
                {
                    // Add the element to the collection of task types
                    inTaskContainers.Add(boeExportTaskElementType, new BOEExportTaskContainer(taskContainer, this.FetchTaskTypeRowFinder(boeElement, boeExportTaskElementType)));
                }
            }
        }

        /// <summary>
        /// Finds a special element in the portrait template that is used to mark the location just before the
        /// repeatable task type details table row.
        /// </summary>
        /// <param name="taskElementContainer">The task element container.</param>
        /// <param name="boeExportTaskElementType">Type of the boe export task element.</param>
        /// <returns>
        /// TableRoe element representing the spot just before the repeatable task type details table row
        /// </returns>
        private TableRow FetchTaskTypeRowFinder(OpenXmlElement taskElementContainer, BOEExportTaskElementType boeExportTaskElementType)
        {
            TableRow toReturn = null;

			// Attempt to find the SdtElement with a special name for the landscape template
			SdtAlias finder = taskElementContainer.Descendants<SdtAlias>().LastOrDefault(a => a.Val.Value == "BOE:TaskTypeRow-" + Enum.GetName(typeof(BOEExportTaskElementType), boeExportTaskElementType));

            // If the special element was found, return it
            if (finder != null)
            {
                toReturn = finder.Ancestors<TableRow>().FirstOrDefault();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the task container for the given Export Task Type
        /// </summary>
        /// <param name="boeExportTaskElementType">The type to retrieve a container for</param>
        /// <param name="inTaskContainers">The task containers.</param>
        /// <returns>
        /// The container that will be populated for the given task type
        /// </returns>
        private BOEExportTaskContainer GetTaskContainer(BOEExportTaskElementType boeExportTaskElementType, Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inTaskContainers)
        {
            BOEExportTaskContainer toReturn = null;

            if (inTaskContainers.Any())
            {
                if (inTaskContainers.ContainsKey(boeExportTaskElementType))
                {
                    toReturn = inTaskContainers[boeExportTaskElementType];

                    if (toReturn.TaskContainer.GetType() == typeof(TableRow))
                    {
                        inTaskContainers[boeExportTaskElementType] = new BOEExportTaskContainer(toReturn.TaskContainer.CloneNode(true), toReturn.TaskTypeRow.CloneNode(true) as TableRow);
                        toReturn.TaskTypeRow.InsertAfterSelf(inTaskContainers[boeExportTaskElementType].TaskContainer);
                        inTaskContainers[boeExportTaskElementType].TaskContainer.InsertAfterSelf(inTaskContainers[boeExportTaskElementType].TaskTypeRow);
                    }
                    else
                    {
                        inTaskContainers[boeExportTaskElementType] = new BOEExportTaskContainer(toReturn.TaskContainer.CloneNode(true));
                        inTaskContainers[boeExportTaskElementType].TaskTypeRow = this.FetchTaskTypeRowFinder(inTaskContainers[boeExportTaskElementType].TaskContainer, boeExportTaskElementType);
                        toReturn.TaskContainer.InsertAfterSelf(inTaskContainers[boeExportTaskElementType].TaskContainer);
                    }

                    inTaskContainers[boeExportTaskElementType].Duplicated = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Removes all empty task containers in the current BOE
        /// </summary>
        /// <param name="inTaskContainers">The task containers.</param>
        private void CleanEmptyTaskContainers(Dictionary<BOEExportTaskElementType, BOEExportTaskContainer> inTaskContainers)
        {
            foreach (KeyValuePair<BOEExportTaskElementType, BOEExportTaskContainer> taskContainer in inTaskContainers)
            {
                if (!taskContainer.Value.Duplicated)
                {
                    this.RemoveSectionTitle(taskContainer.Key, taskContainer.Value.TaskContainer.Ancestors<SdtElement>().FirstOrDefault());
                }

                taskContainer.Value.TaskTypeRow.RemoveIt();
                taskContainer.Value.TaskContainer.RemoveIt();
            }
        }

        private void RemoveSectionTitle(BOEExportTaskElementType taskElementType, OpenXmlElement container)
        {
            if (container == null)
            {
                return;
            }

			// Attempt to find the SdtAlias with a special name for the current task type
			SdtAlias alias = container.Descendants<SdtAlias>().LastOrDefault(s => s.Val.Value.Equals("BOE:SectionTitle-" + Enum.GetName(typeof(BOEExportTaskElementType), taskElementType), StringComparison.CurrentCultureIgnoreCase));

            // If the special alias was found, return its parent SDT Element
            if (alias != null)
            {
				// Get the parent SdtElement
				SdtElement titleContainer = alias.Ancestors<SdtElement>().FirstOrDefault();

                if (titleContainer != null)
                {
                    // Remove the element 
                    titleContainer.RemoveIt();
                }
            }
        }

        /// <summary>
        /// Summary hours rolled up by resource
        /// </summary>
        /// <param name="exportTaskElements">Task element export data</param>
        /// <returns>List of <see cref="LaborRollup"/> containing summary data for each resource</returns>
        private List<LaborRollup> GetLaborHoursRollupByResource(ICollection<BOEExportTaskElement> exportTaskElements)
        {
            List<LaborRollup> results =
                exportTaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ExportFields.ContainsKey(FieldName_TaskTypeDescription))
                    .Select(R => new KeyValuePair<string, decimal>
                        (R.ExportFields.ContainsKey(FieldName_TaskTypeDescription) ? R.ExportFields[FieldName_TaskTypeDescription] : string.Empty, R.Hours.HasValue ? R.Hours.Value : 0L))
                    .GroupBy(x => x.Key).Where(x => x.Any()).Select(x => new LaborRollup() { ResourceDescription = x.Key, Hours = x.Sum(s => s.Value) }).ToList();

            // sort the results by resource description
            results.Sort((x, y) => { return x.ResourceDescription.CompareTo(y.ResourceDescription); });

            return results;
        }

        /// <summary>
        /// Summarizes hours for resources by Resource ID (name) and Resource Description
        /// </summary>
        /// <param name="allExportResourceTypes">Resource Type data to be summarized</param>
        /// <returns>List of summed resource hours grouped by ID and Description</returns>
        private List<LaborResourceRollup> GetLaborResourceHoursRollup(ICollection<BOEExportTaskElementLabor> allExportResourceTypes)
        {
            List<LaborResourceRollup> results = allExportResourceTypes
                .Select(R => new LaborResourceRollup()
                {
                    ResourceDescription = R.ExportFields[FieldName_TaskTypeDescription],
                    ResourceName = R.ExportFields[FieldName_ResourceName],
                    Hours = (R.Hours.HasValue ? Convert.ToInt64(R.Hours.Value) : 0L)
                })
                .GroupBy(x => new { x.ResourceName, x.ResourceDescription })
                .Select(y => new LaborResourceRollup()
                {
                    ResourceName = y.Key.ResourceName,
                    ResourceDescription = y.Key.ResourceDescription,
                    Hours = y.Sum(s => s.Hours)
                }).ToList();

            // sort the results by resource description
            results.Sort((x, y) => { return x.ResourceDescription.CompareTo(y.ResourceDescription); });

            return results;
        }

        /// <summary>
        /// Gets the data needed for the date rollup
        /// </summary>
        /// <param name="taskElementCollection">The task elements containing the data to rollup</param>
        /// <returns>A <see cref="LaborRollupByDate"/> object containing the rolled up data</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private List<LaborRollupByDate> GetRollupByYearData(ICollection<BoeTaskElementDTO> taskElementCollection)
        {
            List<LaborRollupByDate> RollupByDateList = new List<LaborRollupByDate>();

            BoeTaskElementDTO Earliest = taskElementCollection.OrderBy(x => x.StartDate).FirstOrDefault(x => x.StartDate.HasValue);
            BoeTaskElementDTO Latest = taskElementCollection.OrderByDescending(x => x.EndDate).FirstOrDefault(x => x.EndDate.HasValue);

            if (Earliest != null && Latest != null)
            {
                for (int i = Earliest.StartDate.Value.Year; i <= Latest.EndDate.Value.Year; i++)
                {
                    LaborRollupByDate Rollup = new LaborRollupByDate();
                    Rollup.Year = i;
                    Rollup.January = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 1
                        select g.LaborSpreadValue).Sum();
                    Rollup.February = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 2
                        select g.LaborSpreadValue).Sum();
                    Rollup.March = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 3
                        select g.LaborSpreadValue).Sum();
                    Rollup.April = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 4
                        select g.LaborSpreadValue).Sum();
                    Rollup.May = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 5
                        select g.LaborSpreadValue).Sum();
                    Rollup.June = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 6
                        select g.LaborSpreadValue).Sum();
                    Rollup.July = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 7
                        select g.LaborSpreadValue).Sum();
                    Rollup.August = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 8
                        select g.LaborSpreadValue).Sum();
                    Rollup.September = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 9
                        select g.LaborSpreadValue).Sum();
                    Rollup.October = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 10
                        select g.LaborSpreadValue).Sum();
                    Rollup.November = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 11
                        select g.LaborSpreadValue).Sum();
                    Rollup.December = (from e in taskElementCollection
                        from f in e.taskElementLabors
                        where f.SpreadType == SpreadType.Hours
                        from g in f.LaborSpreads
                        where g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 12
                        select g.LaborSpreadValue).Sum();
                    RollupByDateList.Add(Rollup);
                }
            }

            return RollupByDateList;
        }

        /// <summary>
        /// Get data for labor cost symmary
        /// </summary>
        /// <param name="LaborElements">BOE Labor elements</param>
        /// <param name="dateRange">BOE Date Range</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Collection<LaborRollupByDate> GetLaborCostSummaryRollupByYearData(ICollection<BoeTaskElementDTO> LaborElements, DateRange dateRange)
        {
            Collection<LaborRollupByDate> RollupByDateList = new Collection<LaborRollupByDate>();

            for (int i = dateRange.StartDate.Value.Year; i <= dateRange.EndDate.Value.Year; i++)
            {
                LaborRollupByDate Rollup = new LaborRollupByDate();
                Rollup.Year = i;
                Rollup.January = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 1
                    select g.LaborSpreadValue).Sum();

                Rollup.February = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 2
                    select g.LaborSpreadValue).Sum();

                Rollup.March = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 3
                    select g.LaborSpreadValue).Sum();

                Rollup.April = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 4
                    select g.LaborSpreadValue).Sum();

                Rollup.May = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 5
                    select g.LaborSpreadValue).Sum();

                Rollup.June = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 6
                    select g.LaborSpreadValue).Sum();

                Rollup.July = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 7
                    select g.LaborSpreadValue).Sum();

                Rollup.August = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 8
                    select g.LaborSpreadValue).Sum();

                Rollup.September = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 9
                    select g.LaborSpreadValue).Sum();

                Rollup.October = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 10
                    select g.LaborSpreadValue).Sum();

                Rollup.November = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 11
                    select g.LaborSpreadValue).Sum();

                Rollup.December = (from e in LaborElements
                    from f in e.taskElementLabors
                    from g in f.LaborSpreads
                    where f.SpreadType == SpreadType.Cost && g.LaborSpreadDate.Year == i && g.LaborSpreadDate.Month == 12
                    select g.LaborSpreadValue).Sum();

                RollupByDateList.Add(Rollup);
            }

            return RollupByDateList;
        }

        /// <summary>
        /// Get Rollup data for ODC and Travel
        /// </summary>
        /// <param name="ODCElements">BOE ODC elements</param>
        /// <param name="labors">BOE labors</param>
        /// <param name="dateRange">BOE date range</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected Collection<LaborRollupByDate> GetODCTravelCostSummaryRollupByYearData(ICollection<OtherDirectCostDTO> ODCElements,
            Collection<BOEExportTaskElementLabor> labors, DateRange dateRange)
        {
            Collection<LaborRollupByDate> RollupByDateList = new Collection<LaborRollupByDate>();

            if (dateRange != null)
            {
                for (int i = dateRange.StartDate.Value.Year; i <= dateRange.EndDate.Value.Year; i++)
                {
                    LaborRollupByDate Rollup = new LaborRollupByDate();
                    Rollup.Year = i;
                    Rollup.January = (from e in ODCElements
                                         from f in e.ODCTypes
                                         from g in f.ODCSpreads
                                         where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 1
                                         select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                     (from e in labors
                                         where e.ElementType == BOEExportTaskElementType.Travel &&
                                               DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                               DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 1
                                         select e.Cost.Value).Sum();
                    Rollup.February = (from e in ODCElements
                                          from f in e.ODCTypes
                                          from g in f.ODCSpreads
                                          where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 2
                                          select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                      (from e in labors
                                          where e.ElementType == BOEExportTaskElementType.Travel &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 2
                                          select e.Cost.Value).Sum();
                    Rollup.March = (from e in ODCElements
                                       from f in e.ODCTypes
                                       from g in f.ODCSpreads
                                       where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 3
                                       select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                   (from e in labors
                                       where e.ElementType == BOEExportTaskElementType.Travel &&
                                             DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                             DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 3
                                       select e.Cost.Value).Sum();
                    Rollup.April = (from e in ODCElements
                                       from f in e.ODCTypes
                                       from g in f.ODCSpreads
                                       where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 4
                                       select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                   (from e in labors
                                       where e.ElementType == BOEExportTaskElementType.Travel &&
                                             DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                             DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 4
                                       select e.Cost.Value).Sum();
                    Rollup.May = (from e in ODCElements
                                     from f in e.ODCTypes
                                     from g in f.ODCSpreads
                                     where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 5
                                     select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                 (from e in labors
                                     where e.ElementType == BOEExportTaskElementType.Travel &&
                                           DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                           DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 5
                                     select e.Cost.Value).Sum();
                    Rollup.June = (from e in ODCElements
                                      from f in e.ODCTypes
                                      from g in f.ODCSpreads
                                      where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 6
                                      select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                  (from e in labors
                                      where e.ElementType == BOEExportTaskElementType.Travel &&
                                            DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                            DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 6
                                      select e.Cost.Value).Sum();
                    Rollup.July = (from e in ODCElements
                                      from f in e.ODCTypes
                                      from g in f.ODCSpreads
                                      where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 7
                                      select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                  (from e in labors
                                      where e.ElementType == BOEExportTaskElementType.Travel &&
                                            DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                            DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 7
                                      select e.Cost.Value).Sum();
                    Rollup.August = (from e in ODCElements
                                        from f in e.ODCTypes
                                        from g in f.ODCSpreads
                                        where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 8
                                        select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                    (from e in labors
                                        where e.ElementType == BOEExportTaskElementType.Travel &&
                                              DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                              DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 8
                                        select e.Cost.Value).Sum();
                    Rollup.September = (from e in ODCElements
                                           from f in e.ODCTypes
                                           from g in f.ODCSpreads
                                           where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 9
                                           select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                       (from e in labors
                                           where e.ElementType == BOEExportTaskElementType.Travel &&
                                                 DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                                 DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 9
                                           select e.Cost.Value).Sum();
                    Rollup.October = (from e in ODCElements
                                         from f in e.ODCTypes
                                         from g in f.ODCSpreads
                                         where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 10
                                         select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                     (from e in labors
                                         where e.ElementType == BOEExportTaskElementType.Travel &&
                                               DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                               DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 10
                                         select e.Cost.Value).Sum();
                    Rollup.November = (from e in ODCElements
                                          from f in e.ODCTypes
                                          from g in f.ODCSpreads
                                          where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 11
                                          select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                      (from e in labors
                                          where e.ElementType == BOEExportTaskElementType.Travel &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 11
                                          select e.Cost.Value).Sum();
                    Rollup.December = (from e in ODCElements
                                          from f in e.ODCTypes
                                          from g in f.ODCSpreads
                                          where g.ODCSpreadDate.HasValue && g.ODCSpreadDate.Value.Year == i && g.ODCSpreadDate.Value.Month == 12
                                          select ((decimal)g.CostSpreadValue.Value) / 100).Sum() +
                                      (from e in labors
                                          where e.ElementType == BOEExportTaskElementType.Travel &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Year == i &&
                                                DateTime.Parse(e.ExportFields[FieldName_TaskTypeDate]).Month == 12
                                          select e.Cost.Value).Sum();

                    RollupByDateList.Add(Rollup);
                }
            }

            return RollupByDateList;
        }

        private Dictionary<int, List<LaborRollupByDate>> GetODCCostRollup(ICollection<OtherDirectCostDTO> ODCElements, Collection<BOEExportTaskElementLabor> labors)
        {
            Dictionary<int, List<LaborRollupByDate>> RollupByDate = new Dictionary<int, List<LaborRollupByDate>>();
            DateRange ODCElementsDateRange = this.GetODCTravelDateRange(ODCElements, null);

            if (ODCElementsDateRange.StartDate.HasValue && ODCElementsDateRange.EndDate.HasValue)
            {
				List<OtherDirectCostType> ODCElementTypes = ODCElements.SelectMany(x => x.ODCTypes).ToList();
                var ODCGroups = (from f in ODCElementTypes
                    group f by new { f.ResourceID } into g
                    select new { g.Key.ResourceID }).ToList();

                foreach (var item in ODCGroups)
                {
                    RollupByDate.Add(item.ResourceID.Value, new List<LaborRollupByDate>());

                    HashSet<OtherDirectCostSpread> ODCElementTypesByResource = new HashSet<OtherDirectCostSpread>((from f in ODCElementTypes
                        where f.ResourceID == item.ResourceID
                        select f.ODCSpreads).SelectMany(x => x).ToList());

                    for (int i = ODCElementsDateRange.StartDate.Value.Year; i <= ODCElementsDateRange.EndDate.Value.Year; i++)
                    {
                        LaborRollupByDate Rollup = new LaborRollupByDate();
                        Rollup.Resource = labors.First(x => int.Parse(x.ExportFields[FieldName_ResourceID]) == item.ResourceID.Value).ExportFields[FieldName_ResourceName];
                        Rollup.Year = i;
                        Rollup.January = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 1
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.February = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 2
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.March = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 3
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.April = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 4
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.May = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 5
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.June = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 6
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.July = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 7
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.August = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 8
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.September = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 9
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.October = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 10
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.November = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 11
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();
                        Rollup.December = (from e in ODCElementTypesByResource
                            where e.ODCSpreadDate.HasValue && e.ODCSpreadDate.Value.Year == i && e.ODCSpreadDate.Value.Month == 12
                            select ((decimal)e.CostSpreadValue.Value) / 100).Sum();

                        RollupByDate[item.ResourceID.Value].Add(Rollup);
                    }
                }
            }

            return RollupByDate;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Dictionary<int, List<LaborRollupByDate>> GetTravelCostRollup(ICollection<TravelDTO> TravelElements, Collection<BOEExportTaskElement> elements, Collection<ResourceDTO> TravelResources)
        {
            Dictionary<int, List<LaborRollupByDate>> RollupByDate = new Dictionary<int, List<LaborRollupByDate>>();

            DateRange travelElementsDateRange = this.GetODCTravelDateRange(null, TravelElements);
            if (travelElementsDateRange.StartDate.HasValue && travelElementsDateRange.EndDate.HasValue)
            {
				List<TravelTripType> travelTrips = TravelElements.SelectMany(x => x.TravelTrips).ToList();
                var travelGroups = (from f in travelTrips
                    group f by new { f.Segment } into g
                    select new { Segment = g.Key.Segment }).ToList();

				List<BOEExportTaskElement> travelExportElements = elements.Where(g => g.ElementType == BOEExportTaskElementType.Travel).ToList();

                foreach (var item in travelGroups)
                {
                    RollupByDate.Add((int)item.Segment, new List<LaborRollupByDate>());

                    HashSet<TravelTripType> TravelTripsBySegment = new HashSet<TravelTripType>((from f in travelTrips
                        where f.Segment == item.Segment
                        select f).ToList());

                    for (int i = travelElementsDateRange.StartDate.Value.Year; i <= travelElementsDateRange.EndDate.Value.Year; i++)
                    {
                        LaborRollupByDate Rollup = new LaborRollupByDate();
                        ResourceDTO Resource = TravelResources.FirstOrDefault(x => x.Segment == item.Segment);
                        Rollup.Year = i;
                        Rollup.Resource = Resource == null ? string.Empty : Resource.ResourceName;
                        Rollup.January = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 1
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.February = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 2
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.March = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 3
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.April = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 4
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.May = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 5
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.June = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 6
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.July = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 7
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.August = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 8
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.September = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 9
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.October = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 10
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.November = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 11
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();
                        Rollup.December = (from e in TravelTripsBySegment
                            where e.TripDate.Year == i && e.TripDate.Month == 12
                            select travelExportElements.SelectMany(x => x.taskElementLabors).First(f => int.Parse(f.ExportFields[FieldName_TravelTripID]) == e.TravelTripID).Cost.Value).Sum();

                        RollupByDate[(int)item.Segment].Add(Rollup);
                    }
                }
            }

            return RollupByDate;
        }

        /// <summary>
        /// Gets the task hour rollup.
        /// </summary>
        /// <param name="taskElements">The task elements.</param>
        /// <param name="labors">The labors.</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <returns></returns>
        private Dictionary<int, List<LaborRollupByDate>> GetTaskHourRollup(ICollection<BoeTaskElementDTO> taskElements, Collection<BOEExportTaskElementLabor> labors, BOEExportModelView boeExportModelView)
        {
            Dictionary<int, List<LaborRollupByDate>> RollupByDate = new Dictionary<int, List<LaborRollupByDate>>();
            DateRange TaskElementsDateRange = this.GetTaskDateRange(taskElements, boeExportModelView);

            if (TaskElementsDateRange.StartDate.HasValue && TaskElementsDateRange.EndDate.HasValue)
            {
                ICollection<BoeTaskElementDTO> adjustedTaskElements = this.RemoveDiscreteCostResourceEntries(taskElements);
                List<ResourceTypeDto> TaskElementLabors = adjustedTaskElements.SelectMany(x => x.taskElementLabors).ToList();

                var LaborGroups = (from f in TaskElementLabors
                    group f by new { f.ResourceID } into g
                    select new { g.Key.ResourceID }).ToList();

                foreach (var item in LaborGroups)
                {
                    if (item.ResourceID.HasValue)
                    {
                        RollupByDate.Add(item.ResourceID.Value, new List<LaborRollupByDate>());

                        HashSet<ResourceSpreadDto> TaskElementLaborsByResource = new HashSet<ResourceSpreadDto>((from b in TaskElementLabors
                            where b.ResourceID == item.ResourceID
                            select b.LaborSpreads).SelectMany(x => x).ToList());

                        for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
                        {
                            LaborRollupByDate Rollup = new LaborRollupByDate();

                            Rollup.Resource = labors.Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID))
                                .First(x => int.Parse(x.ExportFields[FieldName_ResourceID]) == item.ResourceID.Value)
                                .ExportFields[FieldName_ResourceName];

                            Rollup.Year = i;
                            Rollup.January = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 1
                                select a.LaborSpreadValue).Sum();

                            Rollup.February = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 2
                                select a.LaborSpreadValue).Sum();

                            Rollup.March = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 3
                                select a.LaborSpreadValue).Sum();

                            Rollup.April = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 4
                                select a.LaborSpreadValue).Sum();

                            Rollup.May = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 5
                                select a.LaborSpreadValue).Sum();

                            Rollup.June = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 6
                                select a.LaborSpreadValue).Sum();

                            Rollup.July = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 7
                                select a.LaborSpreadValue).Sum();

                            Rollup.August = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 8
                                select a.LaborSpreadValue).Sum();

                            Rollup.September = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 9
                                select a.LaborSpreadValue).Sum();

                            Rollup.October = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 10
                                select a.LaborSpreadValue).Sum();

                            Rollup.November = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 11
                                select a.LaborSpreadValue).Sum();

                            Rollup.December = (from a in TaskElementLaborsByResource
                                where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 12
                                select a.LaborSpreadValue).Sum();

                            RollupByDate[item.ResourceID.Value].Add(Rollup);
                        }
                    }
                }
            }

            return RollupByDate;
        }

        /// <summary>
        /// Gets the task hour rollup.
        /// </summary>
        /// <param name="taskElements">The task elements.</param>
        /// <param name="labors">The labors.</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <returns></returns>
        private Dictionary<int, List<LaborRollupByDate>> GetTaskHourRollupByCompanyAndLocation(ICollection<BoeTaskElementDTO> taskElements, Collection<BOEExportTaskElementLabor> labors, BOEExportModelView boeExportModelView, BOEExportInputs exportInputs)
        {
            Dictionary<int, List<LaborRollupByDate>> RollupByDate = new Dictionary<int, List<LaborRollupByDate>>();
            int rollupIndex = 0;
            DateRange TaskElementsDateRange = this.GetTaskDateRange(taskElements, boeExportModelView);

            if (TaskElementsDateRange.StartDate.HasValue && TaskElementsDateRange.EndDate.HasValue)
            {
                ICollection<BoeTaskElementDTO> adjustedTaskElements = this.RemoveDiscreteCostResourceEntries(taskElements);
                List<ResourceTypeDto> TaskElementLabors = adjustedTaskElements.SelectMany(x => x.taskElementLabors).ToList();

                CustomFieldDTO companyCustomField = exportInputs.CustomFields.FirstOrDefault(x => x.CustomFieldName.ToLower() == COMPANY_CUSTOM_FIELD.ToLower());
                CustomFieldDTO locationCustomField = exportInputs.CustomFields.FirstOrDefault(x => x.CustomFieldName.ToLower() == LOCATION_CUSTOM_FIELD.ToLower());

                var LaborGroups = (from f in TaskElementLabors
                                   group f by new { f.ResourceID,
                                       company = f.CustomFieldValueContainers.Where(x => x.CustomFieldID == companyCustomField?.Id).FirstOrDefault()?.CustomFieldValueID,
                                       location = f.CustomFieldValueContainers.Where(x => x.CustomFieldID == locationCustomField?.Id).FirstOrDefault()?.CustomFieldValueID,
                                       perfOrgId = f.PerformingOrgID
                                   } into g
                                   select new { g.Key.ResourceID, g.Key.company, g.Key.location, g.Key.perfOrgId }).ToList();

                foreach (var item in LaborGroups)
                {
                    if (item.ResourceID.HasValue)
                    {
                        RollupByDate.Add(rollupIndex, new List<LaborRollupByDate>());

                        HashSet<ResourceSpreadDto> TaskElementLaborsByResource = new HashSet<ResourceSpreadDto>((from b in TaskElementLabors
                                                                                                                 where b.ResourceID == item.ResourceID
                                                                                                                    && b.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == companyCustomField?.Id)?.CustomFieldValueID == item.company
                                                                                                                    && b.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == locationCustomField?.Id)?.CustomFieldValueID == item.location
                                                                                                                    && b.PerformingOrgID == item.perfOrgId
                                                                                                                 select b.LaborSpreads).SelectMany(x => x).ToList());

                        for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
                        {
                            LaborRollupByDate Rollup = this.GetLaborRollupForYear(i, labors, exportInputs, TaskElementLaborsByResource, item.ResourceID.Value, item.company, item.location, item.perfOrgId);
                            
                            RollupByDate[rollupIndex].Add(Rollup);
                        }

                        rollupIndex++;
                    }
                }
            }

            return RollupByDate;
        }

        /// <summary>
        /// Get the Labor Rollup for the given year for GetTaskHourRollupByCompanyAndLocation
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="labors">labors</param>
        /// <param name="exportInputs">export inputs</param>
        /// <param name="taskElementLaborsByResource">Task Element Labors by Resource</param>
        /// <param name="resourceId">Resource Id for labor group</param>
        /// <param name="companyId">CF Id for Company</param>
        /// <param name="locationId">CF Id for Location</param>
        /// <param name="perfOrgId">Performing Org ID for the labor group</param>
        /// <returns>the Labor Rollup for the given year</returns>
        public LaborRollupByDate GetLaborRollupForYear(int year, ICollection<BOEExportTaskElementLabor> labors, BOEExportInputs exportInputs, HashSet<ResourceSpreadDto> taskElementLaborsByResource,
            int resourceId, int? companyId, int? locationId, int? perfOrgId)
        {
            if(exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            LaborRollupByDate rollup = new LaborRollupByDate();

            rollup.Resource = labors.Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID))
                .First(x => int.Parse(x.ExportFields[FieldName_ResourceID]) == resourceId)
                .ExportFields[FieldName_ResourceName];

            rollup.Company = exportInputs.CustomFieldValues.FirstOrDefault(x => x.CustomFieldValueID == companyId)?.CustomFieldValueDescription;

            rollup.Location = exportInputs.CustomFieldValues.FirstOrDefault(x => x.CustomFieldValueID == locationId)?.CustomFieldValueDescription;

            rollup.PerfOrg = labors.Where(x => x.ExportFields.ContainsKey(FieldName_PerfOrgId))
                .First(x => int.Parse(x.ExportFields[FieldName_PerfOrgId]) == perfOrgId)
                .ExportFields[FieldName_PerfOrg];

            rollup.Year = year;
            rollup.January = (from a in taskElementLaborsByResource
                              where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 1
                              select a.LaborSpreadValue).Sum();

            rollup.February = (from a in taskElementLaborsByResource
                               where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 2
                               select a.LaborSpreadValue).Sum();

            rollup.March = (from a in taskElementLaborsByResource
                            where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 3
                            select a.LaborSpreadValue).Sum();

            rollup.April = (from a in taskElementLaborsByResource
                            where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 4
                            select a.LaborSpreadValue).Sum();

            rollup.May = (from a in taskElementLaborsByResource
                          where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 5
                          select a.LaborSpreadValue).Sum();

            rollup.June = (from a in taskElementLaborsByResource
                           where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 6
                           select a.LaborSpreadValue).Sum();

            rollup.July = (from a in taskElementLaborsByResource
                           where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 7
                           select a.LaborSpreadValue).Sum();

            rollup.August = (from a in taskElementLaborsByResource
                             where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 8
                             select a.LaborSpreadValue).Sum();

            rollup.September = (from a in taskElementLaborsByResource
                                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 9
                                select a.LaborSpreadValue).Sum();

            rollup.October = (from a in taskElementLaborsByResource
                              where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 10
                              select a.LaborSpreadValue).Sum();

            rollup.November = (from a in taskElementLaborsByResource
                               where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 11
                               select a.LaborSpreadValue).Sum();

            rollup.December = (from a in taskElementLaborsByResource
                               where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 12
                               select a.LaborSpreadValue).Sum();

            return rollup;
        }

        /// <summary>
        /// Gets the rollup hours or cost for an individual resource
        /// </summary>
        /// <param name="resource">Resource to rollup</param>
        /// <param name="labors">Collection of labors for the BOE</param>
        /// <returns>Returns rollup of data for the resource</returns>
        private List<LaborRollupByDate> GetResourceRollup(ResourceTypeDto resource, Collection<BOEExportTaskElementLabor> labors)
        {
            List<LaborRollupByDate> RollupByDate = new List<LaborRollupByDate>();
            DateRange dateRange = new DateRange(resource.StartDate, resource.EndDate);

            if (dateRange.StartDate.HasValue && dateRange.EndDate.HasValue)
            {
                for (int i = dateRange.StartDate.Value.Year; i <= dateRange.EndDate.Value.Year; i++)
                {
                    LaborRollupByDate Rollup = new LaborRollupByDate();

                    Rollup.Resource = labors.First(x => x.ExportFields.ContainsKey(FieldName_ResourceID)
                                                        && int.Parse(x.ExportFields[FieldName_ResourceID]) == resource.ResourceID.Value)
                        .ExportFields[FieldName_ResourceName];

                    Rollup.Year = i;

                    Rollup.January = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 1
                        select a.LaborSpreadValue).Sum();

                    Rollup.February = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 2
                        select a.LaborSpreadValue).Sum();

                    Rollup.March = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 3
                        select a.LaborSpreadValue).Sum();

                    Rollup.April = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 4
                        select a.LaborSpreadValue).Sum();

                    Rollup.May = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 5
                        select a.LaborSpreadValue).Sum();

                    Rollup.June = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 6
                        select a.LaborSpreadValue).Sum();

                    Rollup.July = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 7
                        select a.LaborSpreadValue).Sum();

                    Rollup.August = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 8
                        select a.LaborSpreadValue).Sum();

                    Rollup.September = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 9
                        select a.LaborSpreadValue).Sum();

                    Rollup.October = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 10
                        select a.LaborSpreadValue).Sum();

                    Rollup.November = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 11
                        select a.LaborSpreadValue).Sum();

                    Rollup.December = (from a in resource.LaborSpreads
                        where a.LaborSpreadDate.Year == i && a.LaborSpreadDate.Month == 12
                        select a.LaborSpreadValue).Sum();

                    RollupByDate.Add(Rollup);
                }
            }

            return RollupByDate;
        }

        /// <summary>
        /// Gets cost rollup for individual ODC resource
        /// </summary>
        /// <param name="odcResource">Resource for the ODC with data to rollup</param>
        /// <param name="labors">Collection of labors for the BOE</param>
        /// <returns>Rollup of data for the ODC element</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private List<LaborRollupByDate> GetODCResourceRollup(OtherDirectCostType odcResource, Collection<BOEExportTaskElementLabor> labors)
        {
            List<LaborRollupByDate> RollupByDate = new List<LaborRollupByDate>();
            DateRange dateRange = new DateRange(odcResource.StartDate, odcResource.EndDate);

            if (dateRange.StartDate.HasValue && dateRange.EndDate.HasValue)
            {
                for (int i = dateRange.StartDate.Value.Year; i <= dateRange.EndDate.Value.Year; i++)
                {
                    LaborRollupByDate Rollup = new LaborRollupByDate();

                    Rollup.Resource = labors.First(x => x.ExportFields.ContainsKey(FieldName_ResourceID)
                                                        && int.Parse(x.ExportFields[FieldName_ResourceID]) == odcResource.ResourceID.Value)
                        .ExportFields[FieldName_ResourceName];

                    Rollup.Year = i;

                    Rollup.January = (from a in odcResource.ODCSpreads
                                         where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 1
                                         select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.February = (from a in odcResource.ODCSpreads
                                          where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 2
                                          select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.March = (from a in odcResource.ODCSpreads
                                       where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 3
                                       select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.April = (from a in odcResource.ODCSpreads
                                       where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 4
                                       select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.May = (from a in odcResource.ODCSpreads
                                     where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 5
                                     select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.June = (from a in odcResource.ODCSpreads
                                      where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 6
                                      select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.July = (from a in odcResource.ODCSpreads
                                      where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 7
                                      select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.August = (from a in odcResource.ODCSpreads
                                        where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 8
                                        select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.September = (from a in odcResource.ODCSpreads
                                           where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 9
                                           select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.October = (from a in odcResource.ODCSpreads
                                         where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 10
                                         select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.November = (from a in odcResource.ODCSpreads
                                          where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 11
                                          select (decimal)a.CostSpreadValue).Sum() / 100;

                    Rollup.December = (from a in odcResource.ODCSpreads
                                          where a.CostSpreadValue.HasValue && a.ODCSpreadDate.Value.Year == i && a.ODCSpreadDate.Value.Month == 12
                                          select (decimal)a.CostSpreadValue).Sum() / 100;

                    RollupByDate.Add(Rollup);
                }
            }

            return RollupByDate;
        }

        private ICollection<BoeTaskElementDTO> RemoveDiscreteCostResourceEntries(ICollection<BoeTaskElementDTO> taskElements)
        {
            ICollection<BoeTaskElementDTO> scrubbedTaskElements = taskElements.DeepClone();

            foreach (BoeTaskElementDTO scrubbedTask in scrubbedTaskElements)
            {
                scrubbedTask.taskElementLabors = scrubbedTask.taskElementLabors.Where(r => r.SpreadType != SpreadType.Cost).ToCollection();
            }

            return scrubbedTaskElements;
        }

        /// <summary>
        /// Gets the rollup data for the GSMO Task Hour Rollup Table
        /// </summary>
        /// <param name="taskElements">Task elements to get the rollup data for</param>
        /// <param name="labors">Collection of labors of the task elements</param>
        /// <param name="fullCompanyName">Bool to determine if the full company name should be used (true) or just company id (false)</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <param name="includePerfOrg">if set to <c>true</c> [include perf org].</param>
        /// <returns>
        /// Rollup data for the GSMO Task Hour Rollup Table
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Dictionary<int, List<GSMOLaborRollupByDate>> GetGSMOTaskHourRollup(Collection<BoeTaskElementDTO> taskElements, Collection<BOEExportTaskElementLabor> labors, bool fullCompanyName, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, bool includePerfOrg)
        {
            Dictionary<int, List<GSMOLaborRollupByDate>> RollupByDate = new Dictionary<int, List<GSMOLaborRollupByDate>>();
            DateRange TaskElementsDateRange = this.GetTaskDateRange(taskElements, boeExportModelView);

            if (TaskElementsDateRange.StartDate.HasValue && TaskElementsDateRange.EndDate.HasValue)
            {
                ICollection<BoeTaskElementDTO> adjustedTaskElements = this.RemoveDiscreteCostResourceEntries(taskElements);
                List<ResourceTypeDto> TaskElementLabors = adjustedTaskElements.SelectMany(x => x.taskElementLabors).ToList();

                if (includePerfOrg)
                {
                    // value only used to determine different rollups, so it's fine to use this instead of the resource id
                    // using resouce id would run into the issue of duplicat keys when also grouping by perf org
                    int rollupKey = 0;

                    var LaborGroups = TaskElementLabors.GroupBy(g => new { ResourceID = g.ResourceID, PerfOrg = g.PerformingOrgID }).Select(s => new { ResourceID = s.Key.ResourceID, PerfOrg = s.Key.PerfOrg }).ToList();

                    foreach (var item in LaborGroups)
                    {
                        rollupKey++;
                        if (item.ResourceID.HasValue && item.PerfOrg.HasValue)
                        {
                            RollupByDate.Add(rollupKey, new List<GSMOLaborRollupByDate>());

                            HashSet<ResourceSpreadDto> TaskElementLaborsByResource = new HashSet<ResourceSpreadDto>((from b in TaskElementLabors
                                where b.ResourceID == item.ResourceID && b.PerformingOrgID == item.PerfOrg
                                select b.LaborSpreads).SelectMany(x => x).ToList());

                            for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
                            {
                                GSMOLaborRollupByDate Rollup = new GSMOLaborRollupByDate();

                                PerformingOrgDTO currentPerfOrg = exportInputs.PerformingOrgsForWsList.FirstOrDefault(p => p.Id == item.PerfOrg.Value);

                                BOEExportTaskElementLabor tempLabor = labors.Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID))
                                    .FirstOrDefault(x => int.Parse(x.ExportFields[FieldName_ResourceID]) == item.ResourceID.Value && x.ExportFields[FieldName_PerfOrg] == currentPerfOrg.PerformingOrgName);

                                Rollup = this.PopulateGSMORollup(tempLabor, TaskElementLaborsByResource, fullCompanyName, i);

                                RollupByDate[rollupKey].Add(Rollup);
                            }
                        }
                    }
                }
                else
                {
                    var LaborGroups = (from f in TaskElementLabors
                        group f by new { f.ResourceID } into g
                        select new { g.Key.ResourceID }).ToList();

                    foreach (var item in LaborGroups)
                    {
                        if (item.ResourceID.HasValue)
                        {
                            RollupByDate.Add(item.ResourceID.Value, new List<GSMOLaborRollupByDate>());

                            HashSet<ResourceSpreadDto> TaskElementLaborsByResource = new HashSet<ResourceSpreadDto>((from b in TaskElementLabors
                                where b.ResourceID == item.ResourceID
                                select b.LaborSpreads).SelectMany(x => x).ToList());

                            for (int i = TaskElementsDateRange.StartDate.Value.Year; i <= TaskElementsDateRange.EndDate.Value.Year; i++)
                            {
                                GSMOLaborRollupByDate Rollup = new GSMOLaborRollupByDate();

                                BOEExportTaskElementLabor tempLabor = labors.Where(x => x.ExportFields.ContainsKey(FieldName_ResourceID))
                                    .FirstOrDefault(x => int.Parse(x.ExportFields[FieldName_ResourceID]) == item.ResourceID.Value);

                                Rollup = this.PopulateGSMORollup(tempLabor, TaskElementLaborsByResource, fullCompanyName, i);

                                RollupByDate[item.ResourceID.Value].Add(Rollup);
                            }
                        }
                    }
                }
            }

            return RollupByDate;
        }

        /// <summary>
        /// populates the rollup data for a row in the GSMO rollup table
        /// </summary>
        /// <param name="labor">labor containing data to be printed</param>
        /// <param name="taskElementLaborsByResource">labor spread data</param>
        /// <param name="fullCompanyName">Bool to determine if the full company name should be used (true) or just company id (false)</param>
        /// <param name="year">Year for the rollup</param>
        /// <returns>rollup for one row</returns>
        private GSMOLaborRollupByDate PopulateGSMORollup(BOEExportTaskElementLabor labor, HashSet<ResourceSpreadDto> taskElementLaborsByResource, bool fullCompanyName, int year)
        {
            GSMOLaborRollupByDate rollup = new GSMOLaborRollupByDate();
            if (labor != null && labor.ExportFields.ContainsKey(FieldName_LaborTypes))
            {
                rollup.LaborType = labor.ExportFields[FieldName_LaborTypes];
            }
            else
            {
                rollup.LaborType = string.Empty;
            }

            if (fullCompanyName && labor != null && labor.ExportFields.ContainsKey(FieldName_LaborTypeCompany))
            {
                rollup.Company = labor.ExportFields[FieldName_LaborTypeCompany];
            }
            else if (!fullCompanyName && labor != null && labor.ExportFields.ContainsKey(FieldName_LaborTypeCompanyId))
            {
                rollup.Company = labor.ExportFields[FieldName_LaborTypeCompanyId];
            }
            else
            {
                rollup.Company = string.Empty;
            }

            if (labor != null && labor.ExportFields.ContainsKey(FieldName_PerfOrg))
            {
                rollup.PerfOrg = labor.ExportFields[FieldName_PerfOrg];
            }
            else
            {
                rollup.PerfOrg = string.Empty;
            }

            rollup.Year = year;
            rollup.January = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 1
                select a.LaborSpreadValue).Sum();

            rollup.February = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 2
                select a.LaborSpreadValue).Sum();

            rollup.March = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 3
                select a.LaborSpreadValue).Sum();

            rollup.April = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 4
                select a.LaborSpreadValue).Sum();

            rollup.May = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 5
                select a.LaborSpreadValue).Sum();

            rollup.June = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 6
                select a.LaborSpreadValue).Sum();

            rollup.July = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 7
                select a.LaborSpreadValue).Sum();

            rollup.August = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 8
                select a.LaborSpreadValue).Sum();

            rollup.September = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 9
                select a.LaborSpreadValue).Sum();

            rollup.October = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 10
                select a.LaborSpreadValue).Sum();

            rollup.November = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 11
                select a.LaborSpreadValue).Sum();

            rollup.December = (from a in taskElementLaborsByResource
                where a.LaborSpreadDate.Year == year && a.LaborSpreadDate.Month == 12
                select a.LaborSpreadValue).Sum();
            return rollup;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected Dictionary<int, List<LaborRollupByDateNew>> GetGSMOLaborTaskCostRollup(ICollection<BoeTaskElementDTO> taskElements, BOEExportInputs exportInputs, Collection<ResourceDTO> resources)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            Dictionary<int, List<LaborRollupByDateNew>> results = new Dictionary<int, List<LaborRollupByDateNew>>();

            List<ResourceTypeDto> allTaskElementResources;

            if (resources == null)
            {
                allTaskElementResources = taskElements.SelectMany(x => x.taskElementLabors).ToList();
            }
            else
            {
                // ignore task resources which are not assigned under the designated Element of Cost
                allTaskElementResources = taskElements.SelectMany(x => x.taskElementLabors).
                    Where(r => r.ResourceID.HasValue && resources.Select(z => z.Id).Contains(r.ResourceID.Value)).ToList();
            }

            IReadOnlyCollection<ResourceDTO> resourcesFromDb = exportInputs.ResourcesUsedInWsBoes;
            
            foreach (ResourceTypeDto resource in allTaskElementResources)
            {
                int resourceID = resource.ResourceID.HasValue ? resource.ResourceID.Value : -1;

                ResourceDTO resourceDto;
                string resourceName;
                List<LaborRollupByDateNew> resourceRollupData;
                
                if (resource.ResourceID.HasValue)
                {
                    resourceDto = resourcesFromDb.First(x => x.Id == resourceID);
                    resourceName = resourceDto.ResourceName;

                    if (results.ContainsKey(resourceID))
                    {
                        resourceRollupData = results[resourceID];
                    }
                    else
                    {
                        resourceRollupData = new List<LaborRollupByDateNew>();
                        results[resourceID] = resourceRollupData;
                    }
                }
                else
                {
                    resourceDto = new ResourceDTO();
                    resourceName = string.Empty;
                    resourceRollupData = new List<LaborRollupByDateNew>();
                }

                foreach (ResourceSpreadDto spread in resource.LaborSpreads)
                {
                    // normalize the spread date for comparison against the resource rate schedule
                    DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

                    int spreadYear = spreadDate.Year;
                    int spreadMonth = spreadDate.Month;

                    LaborRollupByDateNew monthlyValues = resourceRollupData.FirstOrDefault(r => r.Year == spreadYear);
                    if (monthlyValues == null)
                    {
                        monthlyValues = new LaborRollupByDateNew();
                        monthlyValues.Year = spreadYear;
                        monthlyValues.Resource = resourceName;
                        resourceRollupData.Add(monthlyValues);
                    }

                    decimal costValue = 0m;

                    if (resource.SpreadType == SpreadType.Cost)
                    {
                        costValue = spread.LaborSpreadValue;
                    }

                    switch (spreadMonth)
                    {
                        case 1: monthlyValues.January += costValue; break;
                        case 2: monthlyValues.February += costValue; break;
                        case 3: monthlyValues.March += costValue; break;
                        case 4: monthlyValues.April += costValue; break;
                        case 5: monthlyValues.May += costValue; break;
                        case 6: monthlyValues.June += costValue; break;
                        case 7: monthlyValues.July += costValue; break;
                        case 8: monthlyValues.August += costValue; break;
                        case 9: monthlyValues.September += costValue; break;
                        case 10: monthlyValues.October += costValue; break;
                        case 11: monthlyValues.November += costValue; break;
                        case 12: monthlyValues.December += costValue; break;
                        default: break;
                    }
                }
            }

            return results.Where(r => r.Value.Any()).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Get date range for tasks
        /// </summary>
        /// <param name="taskElementCollection">collection of tasks to get date range of</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">ws</exception>
        protected DateRange GetTaskDateRange(ICollection<BoeTaskElementDTO> taskElementCollection, BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            if (taskElementCollection == null || !taskElementCollection.Any())
            {
                return new DateRange();
            }

            DateTime? earliestStartDate = taskElementCollection.Where(x => x.StartDate.HasValue).Select(x => x.StartDate).DefaultIfEmpty().Min();
            DateTime? latestEndDate = taskElementCollection.Where(x => x.EndDate.HasValue).Select(x => x.EndDate).DefaultIfEmpty().Max();

            DateRange toReturn = new DateRange();

            if (earliestStartDate.HasValue && latestEndDate.HasValue)
            {
                try
                {
                    toReturn = new DateRange(earliestStartDate.Value, latestEndDate.Value);
                }
                catch (InvalidOperationException)
                {
                    // get the date from the boe
                    toReturn = new DateRange(boeExportModelView.StartDate, boeExportModelView.EndDate);
                }
            }
            else
            {
                toReturn = new DateRange(boeExportModelView.StartDate, boeExportModelView.EndDate);
            }

            return toReturn;
        }

        /// <summary>
        /// Get date range of material tasks
        /// </summary>
        /// <param name="materialElementCollection">collection of material tasks to get the date range of</param>
        /// <returns></returns>
        protected DateRange GetMaterialTaskDateRange(ICollection<MaterialDTO> materialElementCollection)
        {
            DateTime? earliestStartDate = materialElementCollection.Where(x => x.StartDate.HasValue).Select(x => x.StartDate).DefaultIfEmpty().Min();
            DateTime? latestEndDate = materialElementCollection.Where(x => x.EndDate.HasValue).Select(x => x.EndDate).DefaultIfEmpty().Max();

            if (earliestStartDate.HasValue && latestEndDate.HasValue)
            {
                return new DateRange(earliestStartDate.Value, latestEndDate.Value);
            }
            else
            {
                return new DateRange();
            }
        }

        /// <summary>
        /// Get date range of ODC and Travel elements
        /// </summary>
        /// <param name="ODCElements">ODC elements</param>
        /// <param name="TravelElements">Travel Elements</param>
        /// <returns></returns>
        protected DateRange GetODCTravelDateRange(ICollection<OtherDirectCostDTO> ODCElements, ICollection<TravelDTO> TravelElements)
        {
            DateTime? odcEarliestStart = null;
            DateTime? odcLatestEnd = null;
            DateTime? travelEarliestStart = null;
            DateTime? travelLatestEnd = null;

            if (ODCElements != null)
            {
                odcEarliestStart = ODCElements.Where(x => x.StartDate.HasValue).Select(x => x.StartDate).DefaultIfEmpty().Min();
                odcLatestEnd = ODCElements.Where(x => x.EndDate.HasValue).Select(x => x.EndDate).DefaultIfEmpty().Max();
            }

            if (TravelElements != null)
            {
                travelEarliestStart = TravelElements.Where(x => x.StartDate.HasValue).Select(x => x.StartDate).DefaultIfEmpty().Min();
                travelLatestEnd = TravelElements.Where(x => x.EndDate.HasValue).Select(x => x.EndDate).DefaultIfEmpty().Max();
            }

            if ((odcEarliestStart.HasValue && odcLatestEnd.HasValue) || (travelEarliestStart.HasValue && travelLatestEnd.HasValue))
            {
                DateTime? earliest = odcEarliestStart.HasValue && travelEarliestStart.HasValue
                    ? (odcEarliestStart.Value < travelEarliestStart.Value ? odcEarliestStart.Value : travelEarliestStart.Value)
                    : (odcEarliestStart ?? travelEarliestStart.Value);

                DateTime? latest = odcLatestEnd.HasValue && travelLatestEnd.HasValue
                    ? (odcLatestEnd.Value < travelLatestEnd.Value ? travelLatestEnd.Value : odcLatestEnd.Value)
                    : (odcLatestEnd ?? travelLatestEnd.Value);

                return new DateRange(earliest, latest);
            }
            else
            {
                return new DateRange();
            }
        }

        /// <summary>
        /// Gets the MOQ equation display text for an export todo 31741
        /// </summary>
        /// <param name="taskElement">The task element containing the data needed to construct the moq equation display</param>
        /// <param name="exportInputs">The export's inputs.</param>
        /// <param name="ws">Full WS</param>
        /// <returns>The MOQ equation display text.</returns>
        /// <exception cref="System.ArgumentException">Workspace does not contain the expect boe with id of " + taskElement.BoeID - ws</exception>
        private string GetMOQEquationToDisplay(BOEExportTaskElement taskElement, BOEExportInputs exportInputs, FullWorkspace ws)
        {
            string moqToDisplay = string.Empty;
            BoeDTO boeForTaskElement = exportInputs.AllWorkspaceBoes.FirstOrDefault(x => x.Id == taskElement.BoeID);

            if (boeForTaskElement == null)
            {
                throw new ArgumentException("Workspace does not contain the expect boe with id of " + taskElement.BoeID, nameof(taskElement));
            }

            if (taskElement.ElementType == BOEExportTaskElementType.Labor)
            {
                // need to reformat the equation with variable markers <> so strings can be replaced as a whole 
                // the issue with replacing names with a contains is it's possible to have 2 task variables, DM BASE and SE, and since "SE" is in both task variable names, the contains
                // would try to replace a value for both objects so the "SE" in "DM BASE" would be replaced with a value which is incorrect.
                string FORMAT_MarkedVariableReplacement = "<{0}>";
                string originalMoq = Common.MOQ.Parser.UntagVariables(taskElement.MOQEquation, taskElement.WorkspaceVariables);

                // to find and replace with their values
                ICollection<string> validationResults = Common.MOQ.Parser.Validate(originalMoq).ToList();

                // Set the equation to the first result returned from Validate, which is the
                // re-formatted input equation
                string equation = validationResults.First();
                moqToDisplay = equation;

                // If there were variables found in the equation, we'll replace them with their values
                if (validationResults.Count > 1)
                {
                    // Iterate over all ordinary variables and replace any occurences of those variables
                    // in the equation with the cooresponding value
                    if (taskElement.OrdinaryVariables.Any())
                    {
                        foreach (OrdinaryVariableDto taskvar in taskElement.OrdinaryVariables)
                        {
                            string variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, taskvar.OrdinaryVariableName);
                            Match s = Regex.Match(equation, variableReplacementRegex, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                            if (s.Success)
                            {
                                // need to pull the string from the equation instead of what is stored in the DB since variable names are always stored in UpperCase
                                string variableNameWithCorrectCap = s.ToString().Trim(new char[] { '<', '>' });

                                DataClassForSumOfBOEsCalculation sumOfBoesCalculator = new DataClassForSumOfBOEsCalculation();
                                sumOfBoesCalculator.FillData(new List<OrdinaryVariableDto>() { taskvar }, null, ws);
                                decimal ordinaryVariableValue = this.variableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(taskvar, sumOfBoesCalculator);

                                moqToDisplay = moqToDisplay.Replace(moqToDisplay, Regex.Replace(moqToDisplay, variableReplacementRegex, Convert.ToDecimal(ordinaryVariableValue).ToString("0.#######") + " " + variableNameWithCorrectCap, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT));
                            }
                        }
                    }

                    // Iterate over all workspace variables and replace any occurences of those variables
                    // in the equation with the cooresponding value
                    if (taskElement.WorkspaceVariables.Any())
                    {
                        foreach (WorkspaceVariableDTO workspacevar in taskElement.WorkspaceVariables)
                        {
                            string variableReplacementRegex = string.Format(FORMAT_MarkedVariableReplacement, workspacevar.WorkspaceVariableName);
                            Match s = Regex.Match(equation, variableReplacementRegex, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);
                            if (s.Success)
                            {
                                // need to pull the string from the equation instead of what is stored in the DB since variable names are always stored in UpperCase
                                string variableNameWithCorrectCap = s.ToString().Trim(new char[] { '<', '>' });

								decimal workspaceVariableValue = workspacevar.WorkspaceVariableValue;
                                moqToDisplay = moqToDisplay.Replace(moqToDisplay, Regex.Replace(moqToDisplay, variableReplacementRegex, Convert.ToDecimal(workspaceVariableValue).ToString("0.#######") + " " + variableNameWithCorrectCap, RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT));
                            }
                        }
                    }
                }
            }

            return moqToDisplay;
        }

        /// <summary>
        /// Gets the MOQ total display text for an export.
        /// </summary>
        /// <param name="taskElement">The task element containing the data needed to construct the moq total display.</param>
        /// <param name="exportInputs">The export's inputs.</param>
        /// <param name="allTaskElements">All task elements in the BOE - needed to be able to correctly calculate variable values</param>
        /// <returns>The MOQ total display text.</returns>
        /// <exception cref="GenValidationException">The report could not be generated because there is an invalid MOQ Equation in the workspace. Please run the Validate All BOEs Report to determine the location of this error. Please correct the invalid MOQ Equation before attempting the export again.</exception>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Moq")]
        private string GetMOQTotal(BOEExportTaskElement taskElement, BOEExportInputs exportInputs, IReadOnlyCollection<BoeTaskElementDTO> allTaskElements)
        {
            decimal moqResult = 0;

            try
            {
                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                data.FillData(taskElement.OrdinaryVariables, taskElement.WorkspaceVariables, exportInputs.WbsElements, exportInputs.AllWorkspaceBoes, allTaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);

				string moqResultString = Common.MOQ.Parser.Calculate(taskElement.MOQEquation, taskElement.OrdinaryVariables, taskElement.WorkspaceVariables, this.variableSelectBOEtoSumCalculation, data, exportInputs.Workspace);

                decimal tempMOQResult;
                if (decimal.TryParse(moqResultString, out tempMOQResult))
                {
                    moqResult = tempMOQResult;
                }
            }
            catch
            {
                throw new GenValidationException("The report could not be generated because there is an invalid MOQ Equation in the workspace. Please run the Validate All BOEs Report to determine the location of this error. Please correct the invalid MOQ Equation before attempting the export again.");
            }

            return moqResult == 0 ? string.Empty : " = " + Utilities.FormatStringWithPrecision(moqResult, this.WorkspaceDecimalPrecision);
        }

        /// <summary>
        /// Gets font size to be used in template
        /// </summary>
        /// <param name="defaultSize">default size to be used</param>
        /// <param name="templateType">template being used</param>
        /// <returns>the font size to be used in the template</returns>
        protected virtual string GetFontSize(int defaultSize, ExcelReportTemplateType templateType)
        {
            if (templateType == ExcelReportTemplateType.LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES)
            {
                return "22";
            }
            else if (templateType == ExcelReportTemplateType.LMSI_NISSC_LANDSCAPE_WITH_NON_LABOR_COST)
            {
                return "16";
            }
            else
            {
                return defaultSize.ToString();
            }
        }

        /// <summary>
        /// Gets header font size to be used in template
        /// </summary>
        /// <param name="defaultSize">default size to be used</param>
        /// <param name="templateType">template being used</param>
        /// <returns>the font size to be used in the template</returns>
        protected virtual string GetHeaderFontSize(int defaultSize, ExcelReportTemplateType templateType)
        {
            if (templateType == ExcelReportTemplateType.LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES)
            {
                return "22";
            }
            else if (templateType == ExcelReportTemplateType.LMSI_NISSC_LANDSCAPE_WITH_NON_LABOR_COST)
            {
                return "18";
            }
            else
            {
                return defaultSize.ToString();
            }
        }

        /// <summary>
        /// Gets font to be used in template
        /// </summary>
        /// <param name="defaultFont">default font to be used</param>
        /// <param name="templateType">template being used</param>
        /// <returns>the font to be used in the template</returns>
        private string GetFont(string defaultFont, ExcelReportTemplateType templateType)
        {
            if (templateType == ExcelReportTemplateType.LMSI_NISSC_LANDSCAPE_WITH_NON_LABOR_COST)
            {
                return "Arial";
            }
            else
            {
                return defaultFont;
            }
        }

        /// <summary>
        /// Gets the date range and rollup for the BOE
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelView">export model view for the BOE</param>
        /// <returns>
        /// Collection of labor rolled up by date.
        /// </returns>
        protected virtual Collection<LaborRollupByDate> GetCostRollup(BOEExportInputs exportInputs, BOEExportModelView boeExportModelView)
        {
            Collection<LaborRollupByDate> rollup = null;
            if (exportInputs != null && exportInputs.Workspace != null)
            {
                ICollection<OtherDirectCostDTO> odcElements = exportInputs.Odcs.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();
                ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boeExportModelView.BoeID).ToList();

                if (odcElements.Any() || travelElements.Any())
                {
                    DateRange dateRange = this.GetODCTravelDateRange(odcElements, travelElements);
                    rollup = this.GetODCTravelCostSummaryRollupByYearData(odcElements,
                        boeExportModelView.TaskElements.SelectMany(x => x.taskElementLabors).ToCollection(), dateRange);
                }
            }

            return rollup;
        }

        /// <summary>
        /// Replaces resource name with description for NISSC template
        /// </summary>
        /// <param name="currentLabors">labors with resource name to change</param>
        /// <param name="exportInputs">The export inputs.</param>
        private void ReplaceResNameForNISSC(Collection<BOEExportTaskElementLabor> currentLabors, BOEExportInputs exportInputs)
        {
            foreach (BOEExportTaskElementLabor labor in currentLabors)
            {
                if (labor.ExportFields.ContainsKey(FieldName_ResourceID))
                {
                    int tempResID;

                    if (int.TryParse(labor.ExportFields[FieldName_ResourceID], out tempResID))
                    {
                        ResourceDTO tempResDTO = exportInputs.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == tempResID);

                        if (tempResDTO == null)
                        {
                            tempResDTO = exportInputs.ResourcesForWsResourceListId.FirstOrDefault(x => x.Id == tempResID);
                        }

                        if (tempResDTO == null)
                        {
                            tempResDTO = exportInputs.ResourcesForSystemResourceListId.FirstOrDefault(x => x.Id == tempResID);
                        }

                        labor.ExportFields[FieldName_ResourceName] = tempResDTO.ResourceDesc;
                    }
                }
            }
        }

        /// <summary>
        /// Clones, populates, and inserts a row for the RMS Travel Summary Tables
        /// </summary>
        /// <param name="labor">Data for row</param>
        /// <param name="originalRow">Row to be cloned</param>
        /// <param name="currentRow">current row being populated</param>
        private void PopulateRMSTravelSummaryTableRow(BOEExportTaskElementLabor labor, TableRow originalRow, TableRow currentRow)
        {
            TableRow clonedRow = originalRow.CloneNode(true) as TableRow;
            this.PopulateTaskTypeContent(clonedRow, labor);
            currentRow.InsertAfterSelf(clonedRow);
            currentRow = clonedRow;
        }

        /// <summary>
        /// Append cell to the end of a row, using the formatting and properites of that row
        /// </summary>
        /// <param name="row">Row to append cell to</param>
        /// <param name="text">Text for cell</param>
        private void AppendCellToRow(TableRow row, string text)
        {
            if (row != null)
            {
                // Get cell to use as template for properties - match with last cell in row, the one it will go next to
                TableCell templateCell = row.Descendants<TableCell>().LastOrDefault();

                if (templateCell != null)
                {
                    // Set the cell, paragraph, and run properties
                    TableCellProperties cellProperties = new TableCellProperties(templateCell.TableCellProperties.CloneNode(true));
                    ParagraphProperties paraProperties = templateCell.Descendants<ParagraphProperties>().FirstOrDefault();
                    RunProperties runProperties = templateCell.Descendants<RunProperties>().FirstOrDefault();

                    // Set the text and cell properties
                    TableCell cell = new TableCell();
                    Text cellText = new Text() { Text = text };
                    cell.PrependChild(cellProperties);

                    // Add text and run properties to run
                    Run run = new Run();
                    run.Append(cellText);
                    run.PrependChild(runProperties != null ? runProperties.CloneNode(true) : new RunProperties());

                    // Add run and paragraph properties to paragraph, add paragraph to cell
                    Paragraph paragraph = new Paragraph(run);
                    paragraph.PrependChild(paraProperties != null ? paraProperties.CloneNode(true) : new ParagraphProperties());
                    cell.Append(paragraph);

                    // Append cell to row
                    row.Append(cell);
                }
            }
        }

        /// <summary>
        /// Replaces paragraph tags in html text with div tags
        /// Fixes line spacing issues in export
        /// </summary>
        /// <param name="htmlText">HTML text to replace tags in</param>
        /// <returns>string with paragraph tags replaced with div tags</returns>
        internal string ReplaceParagraphTags(string htmlText)
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                return htmlText;
            }

            return htmlText.Replace("<p", "<div").Replace("</p>", "</div>");
        }
        #endregion Private Functions
    }

    #region Internal Classes

    /// <summary>
    /// Captures rev code rollup data
    /// </summary>
    internal class LaborRollup
    {
        public string RevCode { get; set; }
        public int? ResourceID { get; set; }
        public int? PerfOrgID { get; set; }
        public string PerfOrgName { get; set; }
        public string ResourceDescription { get; set; }
        public decimal? Hours { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// Captures resource hours for OCELOT resource summary table
    /// </summary>
    internal class LaborResourceRollup
    {
        public string ResourceName { get; set; }
        public string ResourceDescription { get; set; }
        public long? Hours { get; set; }
    }

    /// <summary>
    /// Captures date rollup data
    /// </summary>
    public class LaborRollupByDate
    {
        public int Year { get; set; }
        public decimal January { get; set; }
        public decimal February { get; set; }
        public decimal March { get; set; }
        public decimal April { get; set; }
        public decimal May { get; set; }
        public decimal June { get; set; }
        public decimal July { get; set; }
        public decimal August { get; set; }
        public decimal September { get; set; }
        public decimal October { get; set; }
        public decimal November { get; set; }
        public decimal December { get; set; }
        public string Resource { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string PerfOrg { get; set; }
        public decimal Total
        {
            get
            {
                return this.January + this.February + this.March + this.April + this.May + this.June + this.July + this.August + this.September + this.October + this.November + this.December;
            }
        }
    }

    /// <summary>
    /// Captures date rollup data for the GSM-O and SMORS templates
    /// </summary>
    internal class GSMOLaborRollupByDate : LaborRollupByDate
    {
        public string LaborType { get; set; }
    }

    /// <summary>
    /// Enum defining the type of rollup needed
    /// </summary>
    internal enum RollupType
    {
        None = 0,
        Summary = 1,
        TaskDetailSummary = 2,
        MaterialTaskDetailSummary = 3,
        ODCDetail = 4,
        TravelDetail = 5,
        ODCAndTravel = 6
    }

    #endregion
}
