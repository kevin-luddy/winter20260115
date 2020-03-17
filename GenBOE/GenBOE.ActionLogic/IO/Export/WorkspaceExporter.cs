// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.MOQ;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    [ExcludeFromCodeCoverage]
    public abstract class WorkspaceExporter
    {
        public abstract string WORKSPACE_DATA_EXCEL_MAP_PATH { get; }

        protected ICommonDataMapper CommonDataMapper { get; }
        protected IUserDTODataLoader UserDTODataLoader { get; }
        protected string sYes { get { return "Yes"; } }
        protected string sNo { get { return "No"; } }
        protected string sEmpty { get { return string.Empty; } }

        private IPermissionsDTODataLoader permissionsDTOLoader;
        private IResourceDTODataLoader resourceDTODataLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private ICustomFieldValueDTODataLoader customFieldValueDTODataLoader;
        private IBOEStatusReport boeStatusReport;
        private WbsExporter wbsExporter;
        private TravelTripCostCalculation travelTripCostCalculation;
        private ILocationDTODataLoader locationDtoDataLoader;
        private ICLINExporter clinExporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceExporter"/> class.
        /// </summary>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="permissionsDTOLoader">The permissions dto loader.</param>
        /// <param name="resourceDTODataLoader">The resource dto data loader.</param>
        /// <param name="perfOrgLoader">The perf org loader.</param>
        /// <param name="customFieldValueDTODataLoader">The custom field value dto data loader.</param>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="boeStatusReport">The boe status report.</param>
        /// <param name="wbsExporter">The WBS exporter.</param>
        /// <param name="travelTripCostCalculation">The travel trip cost calculation.</param>
        /// <param name="locationDtoDataLoader">The location dto data loader.</param>
        /// <param name="clinExporter">The clin exporter.</param>
        protected WorkspaceExporter(
            ICommonDataMapper commonDataMapper,
            IPermissionsDTODataLoader permissionsDTOLoader,
            IResourceDTODataLoader resourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            ICustomFieldValueDTODataLoader customFieldValueDTODataLoader,
            IUserDTODataLoader userDTODataLoader,
            IBOEStatusReport boeStatusReport,
            WbsExporter wbsExporter,
            TravelTripCostCalculation travelTripCostCalculation,
            ILocationDTODataLoader locationDtoDataLoader,
            ICLINExporter clinExporter)
        {
            this.CommonDataMapper = commonDataMapper;
            this.permissionsDTOLoader = permissionsDTOLoader;
            this.resourceDTODataLoader = resourceDTODataLoader;
            this.perfOrgLoader = perfOrgLoader;
            this.customFieldValueDTODataLoader = customFieldValueDTODataLoader;
            this.UserDTODataLoader = userDTODataLoader;
            this.boeStatusReport = boeStatusReport;
            this.wbsExporter = wbsExporter;
            this.travelTripCostCalculation = travelTripCostCalculation;
            this.locationDtoDataLoader = locationDtoDataLoader;
            this.clinExporter = clinExporter;
        }

        /// <summary>
        /// Exports workspace data to Excel.
        /// </summary>
        /// <param name="inTemplateFileLocation">Location of the excel template file.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="metricNameTaskElementMappingDTO">Mapping of task element to historical metric names.</param>
        /// <returns>Name of the generated report file.</returns>
        public string ExportToExcelFile(string inTemplateFileLocation, BOEExportInputs exportInputs, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO, ICollection<PickListDto> contractTypes)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }
            
            // Check inputs
            if (inTemplateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(inTemplateFileLocation));
            }

            string toReturn = ExcelUtilities.CopyExcelTemplateFile(inTemplateFileLocation);
            
            this.DuplicateCustomFieldColumns(toReturn, exportInputs);

            this.SetAutofilterRange(toReturn);

            ICollection<ExcelExportWorksheet> sheets = this.GatherWorkspaceExportData(exportInputs, metricNameTaskElementMappingDTO, contractTypes);

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(toReturn, false, sheets, this.GetStartRows(), FullObjectHelper.ShowEquivalentPersonsOption && exportInputs.Workspace.IsUsingEquivalentPerson);
            
            if (!exportInputs.FullWorkspace.Travels.Any())
            {
                // hide travel sheets if there are no travels 
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(toReturn, true))
                {
                    ExcelUtilities.HideWorksheets(document, new List<string>() { "Travel Unit Cost", "Travel Extended Cost" });
                }
            }
            else
            {
                int zoneRowCount = this.GetZoneRowCount(exportInputs.FullWorkspace);
                if (zoneRowCount > 0)
                {
                    this.ApplyNonzoneFormattingToTravelSheets(toReturn, zoneRowCount);
                }
            }
            
            return toReturn;
        }

        /// <summary>
        /// Gets the start rows for the sheets in the Workspace Data Export
        /// </summary>
        /// <returns>int array of start rows</returns>
        protected virtual int?[] GetStartRows()
        {
            return new int?[] { null, null, 1, null, null, null, null, null };
        }

        /// <summary>
        /// Applies the remaining formatting to the Travel sheets
        /// </summary>
        /// <param name="filePath">The file path for the export</param>
        /// <param name="zoneTripRowCount">Zone Trip Row Count</param>
        protected virtual void ApplyNonzoneFormattingToTravelSheets(string filePath, int zoneTripRowCount)
        {
            // Do nothing for ssc/default
        }

        /// <summary>
        /// Gets the count of Zone rows, which is the number of unique trips
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>The number of zone rows</returns>
        protected virtual int GetZoneRowCount(FullWorkspace workspace)
        {
            // always 0 for ssc because no zone travel
            return 0;
        }

        /// <summary>
        /// Sets the autofilter range.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        private void SetAutofilterRange(string templateFileLocation)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(templateFileLocation, true))
            {
                WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(document, "BOEs");

                if (worksheetPart != null)
                {
                    Worksheet worksheet = worksheetPart.Worksheet;

                    Column lastColumn = worksheet.Descendants<Column>().LastOrDefault();

                    if (lastColumn != null && lastColumn.Min.HasValue)
                    {
                        string lastColumnName = ExcelUtilities.GetColumnNameFromColumnIndex((int)lastColumn.Min.Value - 1);
                        AutoFilter autoFilter = worksheetPart.Worksheet.Descendants<AutoFilter>().FirstOrDefault();

                        if (autoFilter != null && autoFilter.Reference.HasValue)
                        {
                            string[] filterReference = autoFilter.Reference.Value.Split(':');

                            if (filterReference.Length == 2)
                            {
                                autoFilter.Reference = filterReference[0] + ":" + lastColumnName + ExcelUtilities.ParseRowIndex(filterReference[1]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Duplicates the custom field columns.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="exportInputs">The export inputs.</param>
        private void DuplicateCustomFieldColumns(string templateFileLocation, BOEExportInputs exportInputs)
        {
            IReadOnlyCollection<CustomFieldDTO> customFields = exportInputs.CustomFields;

            string[] boeCustomFieldNames = customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay).Select(c => c.CustomFieldName).ToArray();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(templateFileLocation, true))
            {
                if (boeCustomFieldNames.Any())
                {
                    ExcelUtilities.DuplicateColumn(document, "BOE & Resource Combo", "BOE Custom Field", boeCustomFieldNames);
                    ExcelUtilities.DuplicateColumn(document, "BOEs", "BOE Custom Field", boeCustomFieldNames);
                }
                else
                {
                    ExcelUtilities.RemoveColumn(document, "BOE & Resource Combo", "BOE Custom Field");
                    ExcelUtilities.RemoveColumn(document, "BOEs", "BOE Custom Field");
                }
            }

            string[] taskCustomFieldNames = customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay).Select(c => c.CustomFieldName).ToArray();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(templateFileLocation, true))
            {
                if (taskCustomFieldNames.Any())
                {
                    ExcelUtilities.DuplicateColumn(document, "BOE & Resource Combo", "Task Custom Field", taskCustomFieldNames);
                    ExcelUtilities.DuplicateColumn(document, "BOEs", "Task Custom Field", taskCustomFieldNames);
                }
                else
                {
                    ExcelUtilities.RemoveColumn(document, "BOE & Resource Combo", "Task Custom Field");
                    ExcelUtilities.RemoveColumn(document, "BOEs", "Task Custom Field");
                }
            }

            string[] resourceCustomFieldNames = customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay).Select(c => c.CustomFieldName).ToArray();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(templateFileLocation, true))
            {
                if (resourceCustomFieldNames.Any())
                {
                    ExcelUtilities.DuplicateColumn(document, "BOE & Resource Combo", "Resource Custom Field", resourceCustomFieldNames);
                    ExcelUtilities.DuplicateColumn(document, "BOEs", "Resource Custom Field", resourceCustomFieldNames);
                }
                else
                {
                    ExcelUtilities.RemoveColumn(document, "BOE & Resource Combo", "Resource Custom Field");
                    ExcelUtilities.RemoveColumn(document, "BOEs", "Resource Custom Field");
                }
            }
        }

        /// <summary>
        /// Gets all workspace data required for the export.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="metricNameTaskElementMappingDTO">Mapping of task element to historical metric name.</param>
        /// <returns>
        /// Excel workspace data.
        /// </returns>
        private ICollection<ExcelExportWorksheet> GatherWorkspaceExportData(BOEExportInputs exportInputs, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO, ICollection<PickListDto> contractTypes)
        {
            List<ExcelExportWorksheet> toReturn = new List<ExcelExportWorksheet>();

            if (exportInputs.Workspace.Id > 0)
            {
                // refresh in use flag
                this.customFieldValueDTODataLoader.RefreshCustomFieldInUseByWorkspaceID(exportInputs.FullWorkspace.Id);

                toReturn.Add(this.GetBOEResourceComboSheetExportData(exportInputs, metricNameTaskElementMappingDTO));
                toReturn.Add(this.GetBOESheetExportData(exportInputs, metricNameTaskElementMappingDTO));
                toReturn.Add(this.GetBOEStatusSheetExportData(exportInputs));
                toReturn.Add(this.GetResourceSpreadsSheetExportData(exportInputs));
                toReturn.Add(this.GetWBSSheetExportData(exportInputs));
                toReturn.Add(this.GetCLINsSheetExportData(exportInputs, contractTypes));
                toReturn.Add(this.GetUserPermissionsSheetExportData(exportInputs));
                toReturn.Add(this.GetWorkspaceIdentificationSheetExportData(exportInputs));

                ExcelExportWorksheet travelUnitCostSheetData = this.GetTravelUnitCostSheetExportData(exportInputs.FullWorkspace);
                if (travelUnitCostSheetData != null)
                {
                    travelUnitCostSheetData.WorksheetName = "Travel Unit Cost";
                    toReturn.Add(travelUnitCostSheetData);
                }

                ExcelExportWorksheet travelExtendedCostSheetData = this.GetTravelExtendedCostSheetExportData(exportInputs.FullWorkspace);
                if (travelExtendedCostSheetData != null)
                {
                    travelExtendedCostSheetData.WorksheetName = "Travel Extended Cost";
                    toReturn.Add(travelExtendedCostSheetData);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all worksheet data for the BOE data sheet.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="metricNameTaskElementMappingDTO">Task element to historical metric mapping.</param>
        /// <returns>
        /// BOE Worksheet data.
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        private ExcelExportWorksheet GetBOESheetExportData(BOEExportInputs exportInputs, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO)
        {
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet("BOEs");

            // Bulk load all data.
            IReadOnlyCollection<CustomFieldDTO> workspace_customFields = exportInputs.CustomFields;
            ICollection<CustomFieldValueDTO> workspaceCustomFieldValues = new List<CustomFieldValueDTO>();
            ICollection<BoeDTO> boes = exportInputs.Boes.ToCollection();

            if (workspace_customFields.Any())
            {
                workspaceCustomFieldValues = this.customFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(workspace_customFields.Select(i => i.Id).ToCollection<int>());
            }

            IDictionary<int, SpreadCurveModelView> allSpreadCurves = this.CommonDataMapper.getSpreadCurveDictionary();
            IDictionary<int, OtherDirectCostSpreadCurveModelView> allOdcSpreadCurves = this.CommonDataMapper.getOdcSpreadCurveDictionary();
            
            HashSet<BoeTaskElementDTO> allTaskElements = new HashSet<BoeTaskElementDTO>(exportInputs.TaskElements);
            HashSet<OtherDirectCostDTO> allOdcs = new HashSet<OtherDirectCostDTO>(exportInputs.Odcs);
            HashSet<MaterialDTO> allMaterials = new HashSet<MaterialDTO>(exportInputs.Materials);
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(exportInputs.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(exportInputs.WbsElements);
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(exportInputs.Travels);
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(exportInputs.TravelTrips);
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(exportInputs.PerDiemsForTravelTrips);
            HashSet<LocationDTO> allDepartures = new HashSet<LocationDTO>(this.locationDtoDataLoader.GetByIds(allTravelTrips.Select(i => i.DepartureLocationID).ToCollection()));
            HashSet<LocationDTO> allDestinations = new HashSet<LocationDTO>(this.locationDtoDataLoader.GetByIds(allTravelTrips.Select(i => i.DestinationLocationID).ToCollection()));
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(exportInputs.EscalationRates);
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(exportInputs.MiscTravelRatesForTravelTrips);

            foreach (BoeDTO boe in boes)
            {
                // show all tasks regardless if it has a resource type populated.
                List<BoeTaskElementDTO> tasks = allTaskElements.Where(i => i.BoeID == boe.Id).ToList();
                ICollection<OtherDirectCostDTO> odcs = allOdcs.Where(i => i.BoeID == boe.Id).ToCollection();
                ICollection<TravelDTO> travelElements = allTravels.Where(i => i.BoeID == boe.Id).ToCollection();
                ICollection<MaterialDTO> materialElements = allMaterials.Where(i => i.BoeID == boe.Id).ToCollection();
               
                // Sum up all the trip data for the BOE.
                decimal travelElementsSum = 0;
                foreach (TravelDTO travel in travelElements)
                {
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);
                        travelElementsSum += this.travelTripCostCalculation.CalculateTravelCost(tripType, exportInputs.FullWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal;
                    }
                }

                WbsDTO boe_WBS = allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
                ClinDTO boe_CLIN = allClins.FirstOrDefault(i => i.Id == boe.CLINID);
                decimal boe_HoursSum = tasks.Sum(t => t.taskElementLabors.Where(l => l.SpreadType == SpreadType.Hours && l.ValueSpread.HasValue).Sum(l => l.ValueSpread.Value));
                decimal boe_CostSum =
                    (decimal)(tasks.Sum(t => t.taskElementLabors.Where(l => l.SpreadType == SpreadType.Cost && l.ValueSpread.HasValue).Sum(l => l.ValueSpread.Value)))
                    + (((decimal)(odcs.Sum(odc => odc.ODCTypes.Sum(odcTypes => odcTypes.ODCSpreads.Sum(odcSpreads => odcSpreads.CostSpreadValue))))) / 100)
                    + travelElementsSum;
                string boe_StartDate = boe.StartDate.ToString("MM/yyyy");
                string boe_EndDate = boe.EndDate.ToString("MM/yyyy");

                List<string> row = new List<string>();

                row.AddRange(
                    new string[]{
                        boe.Id.ToString(),
                        boe.isMaterial ?  "Material BOE": "BOE",
                        boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                        boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                        boe.Title != null ? boe.Title : this.sEmpty,
                        boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                        boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                        boe_StartDate,
                        boe_EndDate}.AsEnumerable());

                foreach (CustomFieldDTO boeCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay))
                {
                    ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == boeCustomField.Id).ToCollection<CustomFieldValueDTO>();
                    CustomFieldValueDTO customFieldValueForBOE = customFieldValues.FirstOrDefault(c => boe.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID)); 
                    row.Add(customFieldValueForBOE != null ? customFieldValueForBOE.CustomFieldValueDescription : this.sEmpty);
                }

                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe_HoursSum.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision)));
                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe_CostSum));
                toReturn.Add(row);


                #region Task Elements

                //add any Labor tasks 
                foreach (BoeTaskElementDTO task in tasks)
                {
                    row = new List<string>();

                    decimal isDecimal;

                    // Remove comma from a decimal value otherwise Excel will generate a Numbers stored as text error when opening.
                    // Ex: 1,000 will be exported as 1000
                    if (!string.IsNullOrEmpty(task.MOQHoursEquation) && decimal.TryParse(task.MOQHoursEquation, out isDecimal))
                    {
                        task.MOQHoursEquation = isDecimal.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision));
                    }

                    // Get the task ID
                    string taskID = task.BOETaskID;

                    string historicalMetricString = metricNameTaskElementMappingDTO.GetMetricNamesByTaskElementId(task.Id);

                    row.AddRange(
                        new string[] {
                            boe.Id.ToString(),
                            (task.TaskElementType == TaskElementType.Labor) ? "Task" : this.sEmpty,
                            boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                            boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                            boe.Title != null ? boe.Title : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                            boe_StartDate,
                            boe_EndDate
                        });

                    int emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                    for (int i = 0;i < emptyCellsToAdd;i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    // Task Description and Task MOQ Text are now HTML formatted rich text. We need to get plain text out of them
                    string descriptionRteOverride = BOEExportConverter.GetRteOverride(boe.Id, task.Id, task.Description, RteTemplateSource.BoeDescription, exportInputs.RTETemplatesOverrides);
                    string moqRteOverride = BOEExportConverter.GetRteOverride(task.BoeID, task.Id, task.MOQText, RteTemplateSource.TaskMOQ, exportInputs.RTETemplatesOverrides);

                    ICollection<string> plainText = RTEUtilities.TurnHTMLIntoPlainText(new List<string>() { descriptionRteOverride, moqRteOverride });
                    string taskDescription = plainText.ElementAt(0);
                    string taskMOQText = plainText.ElementAt(1);

                    string[] taskpart1 = new string[] {
                        this.sEmpty, this.sEmpty,
                            boe.IsMultiClinWbs ? this.sYes : this.sNo,
                            boe.isMaterial ?  this.sYes: this.sNo,
                            taskID,
                            task.TaskTitle,
                            taskDescription,
                            (task.TaskElementType == TaskElementType.Labor)
                                ? Parser.UntagVariables(task.MOQHoursEquation, exportInputs.WorkspaceVariables.ToList())
                                : this.sEmpty,
                            task.MOQType.GetDescription(),
                            taskMOQText,
                            historicalMetricString,
                            task.StartDate.HasValue ? task.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            task.EndDate.HasValue ? task.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                        };

                    row.AddRange(taskpart1);

                    foreach (CustomFieldDTO taskCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay))
                    {
                        ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == taskCustomField.Id).ToCollection<CustomFieldValueDTO>();
                        CustomFieldValueDTO customFieldValueForTask = customFieldValues.FirstOrDefault(c => task.CustomFieldValueContainers.Select(t => t.CustomFieldValueID).Contains(c.CustomFieldValueID));
                        row.Add(customFieldValueForTask != null ? customFieldValueForTask.CustomFieldValueDescription : this.sEmpty);
                    }

                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + task.taskElementLabors.Where(l => l.SpreadType == SpreadType.Hours).Sum(l => l.ValueSpread).ToString());

                    // Sum up all the Cost type resources.
                    decimal taskElementCostTotal = task.taskElementLabors.Where(l => l.SpreadType == SpreadType.Cost).Sum(l => l.ValueSpread.HasValue ? l.ValueSpread.Value : 0);
                    
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, taskElementCostTotal));

                    toReturn.Add(row);

                    HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this.resourceDTODataLoader.GetByIds(task.taskElementLabors.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
                    HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(task.taskElementLabors.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

                    foreach (ResourceTypeDto resourceType in task.taskElementLabors)
                    {
                        row = new List<string>();
                        string wbsNumber, wbsTitle, clinNumber, clinTitle;

                        //Use resource level WBS/CLIN if BOE is set to Multi, otherwise display BOEs WBS/CLIN
                        if (boe.IsMultiClinWbs)
                        {
                            WbsDTO resourceWbs = resourceType.WBSID != null ? allWbs.FirstOrDefault(w => w.Id == resourceType.WBSID) : null;
                            ClinDTO resourceClin = resourceType.CLINID != null ? allClins.FirstOrDefault(c => c.Id == resourceType.CLINID) : null;

                            wbsNumber = resourceWbs != null ? resourceWbs.WbsNumber : this.sEmpty;
                            wbsTitle = resourceWbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceWbs.WbsTitle : this.sEmpty;
                            clinNumber = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinNumber : this.sEmpty;
                            clinTitle = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinTitle : this.sEmpty; 
                        }
                        else
                        { 
                            wbsNumber = boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty;
                            wbsTitle = boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty;
                            clinNumber = boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty;
                            clinTitle = boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty;
                        }

                        row.AddRange(
                            new string[] {
                                boe.Id.ToString(),
                                "Resource Type",
                                wbsNumber,
                                wbsTitle,
                                boe.Title != null ? boe.Title : this.sEmpty,
                                clinNumber,
                                clinTitle,
                                boe_StartDate,
                                boe_EndDate
                            });

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        row.AddRange(taskpart1);

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        ResourceDTO aResource = resourceType.ResourceID.HasValue ? resourcesFromDb.First(x => x.Id == resourceType.ResourceID.Value) : new ResourceDTO();
                        PerformingOrgDTO perfOrg = resourceType.PerformingOrgID.HasValue ? perfOrgsFromDb.First(x => x.Id == resourceType.PerformingOrgID.Value) : new PerformingOrgDTO();
                        string percentSpread = this.sEmpty;

                        if (resourceType.SpreadType == SpreadType.Hours)
                        {
                            if (task.TaskElementType == TaskElementType.Labor && resourceType.PercentSpread.HasValue)
                            {
                                percentSpread = resourceType.PercentSpread.Value.ToString();
                            }
                            else
                            {
                                percentSpread = this.sEmpty;
                            }
                        }


                        row.AddRange(
                            new string[] {
                                this.sEmpty, this.sEmpty,
                                resourceType.ResourceID.HasValue ? aResource.ElementOfCost.ToString() : this.sEmpty,
                                resourceType.ResourceID.HasValue ? aResource.ResourceDesc : this.sEmpty,
                                resourceType.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty,
                                resourceType.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                                resourceType.ResourceID.HasValue ? aResource.ResourceName : this.sEmpty,
                                resourceType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgName : this.sEmpty,
                                resourceType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgDesc : this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty,
                                resourceType.StartDate.HasValue ? resourceType.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                                resourceType.EndDate.HasValue ? resourceType.EndDate.Value.ToString("MM/yyyy") : this.sEmpty,
                                resourceType.SpreadCurveID.HasValue ? allSpreadCurves[(int)resourceType.SpreadCurveID.Value].SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(exportInputs.Workspace)) : this.sEmpty,
                                percentSpread,
                                (resourceType.SpreadType == SpreadType.Hours && resourceType.ValueSpread.HasValue)
                                    ? CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + resourceType.ValueSpread.Value.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision))
                                    : this.sEmpty,
                                (resourceType.SpreadType == SpreadType.Cost && resourceType.ValueSpread.HasValue)
                                    ? CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + resourceType.ValueSpread.Value.ToString(Utilities.CostPrecisionFormattingString(exportInputs.Workspace.CostDecimalPrecision))
                                    : this.sEmpty
                            });

                        foreach (CustomFieldDTO resourceCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
                        {
                            ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == resourceCustomField.Id).ToCollection<CustomFieldValueDTO>();
                            CustomFieldValueDTO customFieldValueForResource = customFieldValues.FirstOrDefault(c => resourceType.CustomFieldValueContainers.Select(r => r.CustomFieldValueID).Contains(c.CustomFieldValueID));
                            row.Add(customFieldValueForResource != null ? customFieldValueForResource.CustomFieldValueDescription : this.sEmpty);
                        }

                        toReturn.Add(row);
                    }
                }
                #endregion task

                #region Materials

                //add any Material tasks
                foreach (MaterialDTO material in materialElements)
                {
                    row = new List<string>();

                    row.AddRange(
                        new string[] {
                            boe.Id.ToString(),
                            "Task",
                            boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                            boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                            boe.Title != null ? boe.Title : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                            boe_StartDate,
                            boe_EndDate
                        });

                    int emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                    for (int i = 0; i < emptyCellsToAdd; i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    ICollection<string> plainText = RTEUtilities.TurnHTMLIntoPlainText(new List<string>() { material.TaskDescription, material.MoqText });
                    string taskDescription = plainText.ElementAt(0);
                    string MOQText = plainText.ElementAt(1);

                    string[] materialPart1 = new string[] {
                        this.sEmpty, this.sEmpty,
                            boe.IsMultiClinWbs ? this.sYes : this.sNo,
                            boe.isMaterial ?  this.sYes : this.sNo,
                            material.TaskID,
                            material.TaskTitle,
                            taskDescription, this.sEmpty, this.sEmpty,
                            MOQText, this.sEmpty,
                            material.StartDate.HasValue ? material.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            material.EndDate.HasValue ? material.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                        };

                    row.AddRange(materialPart1);

                    emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay);
                    for (int i = 0; i < emptyCellsToAdd; i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    row.Add(this.sEmpty);
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, 0m));
                    toReturn.Add(row);
                }

                #endregion Materials

                #region ODC
                //add any Labor tasks
                foreach (OtherDirectCostDTO odc in odcs)
                {
                    row = new List<string>();

                    row.AddRange(
                        new string[] {
                            boe.Id.ToString(),
                            "Task",
                            boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                            boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                            boe.Title != null ? boe.Title : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                            boe_StartDate,
                            boe_EndDate
                        });

                    int emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                    for (int i = 0; i < emptyCellsToAdd; i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    ICollection<string> plainText = RTEUtilities.TurnHTMLIntoPlainText(new List<string>() { odc.TaskDescription, odc.MoqText });
                    string taskDescription = plainText.ElementAt(0);
                    string MOQText = plainText.ElementAt(1);

                    string[] odcPart1 = new string[] {
                        this.sEmpty, this.sEmpty,
                            boe.IsMultiClinWbs ? this.sYes : this.sNo,
                            boe.isMaterial ?  this.sYes : this.sNo,
                            odc.Id.ToString(),
                            odc.TaskTitle,
                            taskDescription, this.sEmpty, this.sEmpty,
                            MOQText, this.sEmpty,
                            odc.StartDate.HasValue ? odc.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            odc.EndDate.HasValue ? odc.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                        };

                    row.AddRange(odcPart1);

                    emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay);
                    for (int i = 0; i < emptyCellsToAdd; i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    row.Add(this.sEmpty);
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, ((decimal)(odc.ODCTypes.Sum(odct => odct.ODCSpreads.Sum(odcsp => odcsp.CostSpreadValue))) / 100)));
                    toReturn.Add(row);

                    HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this.resourceDTODataLoader.GetByIds(odc.ODCTypes.Where(x=> x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
                    HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(odc.ODCTypes.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

                    foreach (OtherDirectCostType odcType in odc.ODCTypes)
                    {
                        row = new List<string>();

                        row.AddRange(
                            new string[] {
                                boe.Id.ToString(),
                                "ODC",
                                boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                                boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                                boe.Title != null ? boe.Title : this.sEmpty,
                                boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                                boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                                boe_StartDate,
                                boe_EndDate
                            });

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        row.AddRange(odcPart1);

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        ResourceDTO aResource = odcType.ResourceID.HasValue ? resourcesFromDb.First(x => x.Id == odcType.ResourceID.Value) : new ResourceDTO();
                        PerformingOrgDTO perfOrg = odcType.PerformingOrgID.HasValue ? perfOrgsFromDb.First(x => x.Id == odcType.PerformingOrgID.Value) : new PerformingOrgDTO();
                        OtherDirectCostSpreadCurveModelView spreadCurve = null;
                        string spreadCurveName = odcType.SpreadCurve.ToString();
                        if (allOdcSpreadCurves.TryGetValue((int)odcType.SpreadCurve, out spreadCurve))
                        {
                            spreadCurveName = spreadCurve.SpreadCurveName;
                        }

                        row.AddRange(
                            new string[] {
                                this.sEmpty, this.sEmpty,
                            odcType.ResourceID.HasValue ? aResource.ElementOfCost.ToString() : this.sEmpty,
                            odcType.ResourceID.HasValue ? aResource.ResourceDesc : this.sEmpty,
                            odcType.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty,
                            odcType.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                            odcType.ResourceID.HasValue ? aResource.ResourceName : this.sEmpty,
                            odcType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgName : this.sEmpty,
                            odcType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgDesc : this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty,
                            odcType.StartDate.HasValue ? odcType.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            odcType.EndDate.HasValue ? odcType.EndDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            spreadCurveName, this.sEmpty, this.sEmpty,
                            CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, (((decimal)odcType.ODCSpreads.Sum(odcs2 => odcs2.CostSpreadValue))/100))
                            });

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        toReturn.Add(row);
                    }
                }
                #endregion ODC

                #region Travel
                //add any Labor tasks
                foreach (TravelDTO travel in travelElements)
                {
                    row = new List<string>();

                    row.AddRange(
                        new string[] {
                            boe.Id.ToString(),
                            "Task",
                            boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                            boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                            boe.Title != null ? boe.Title : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                            boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                            boe_StartDate,
                            boe_EndDate
                        });

                    int emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                    for (int i = 0; i < emptyCellsToAdd; i++)
                    {
                        row.Add(this.sEmpty);
                    }

                    ICollection<string> plainText = RTEUtilities.TurnHTMLIntoPlainText(new List<string>() { travel.Description });
                    string taskDescription = plainText.ElementAt(0);

                    string[] travelPart1 = new string[] {
                        this.sEmpty, this.sEmpty,
                            boe.IsMultiClinWbs ? this.sYes : this.sNo,
                            boe.isMaterial ?  this.sYes : this.sNo,
                            travel.TaskID,
                            travel.TaskTitle,
                            taskDescription, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty,
                            travel.StartDate.HasValue ? travel.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                            travel.EndDate.HasValue ? travel.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                        };

                    row.AddRange(travelPart1);

                 
                    foreach (CustomFieldDTO taskCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay))
                    {
                        ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == taskCustomField.Id).ToCollection<CustomFieldValueDTO>();
                        CustomFieldValueDTO customFieldValueForTask = customFieldValues.FirstOrDefault(c => travel.CustomFieldValueContainers.Select(t => t.CustomFieldValueID).Contains(c.CustomFieldValueID));
                        row.Add(customFieldValueForTask != null ? customFieldValueForTask.CustomFieldValueDescription : this.sEmpty);
                    }

                    row.Add(this.sEmpty);
                    // Sum the trip data for the travel element.
                    decimal travelTripsSum = 0;
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);
                        travelTripsSum += this.travelTripCostCalculation.CalculateTravelCost(tripType, exportInputs.FullWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal;
                    }

                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, travelTripsSum));
                    toReturn.Add(row);

                    HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(travel.TravelTrips.Select(x => x.PerfOrgID).Distinct().ToList()));

                    foreach (TravelTripType travelTrip in travel.TravelTrips)
                    {
                        row = new List<string>();

                        row.AddRange(
                            new string[] {
                                boe.Id.ToString(),
                                "Travel",
                                boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                                boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                                boe.Title != null ? boe.Title : this.sEmpty,
                                boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                                boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                                boe_StartDate,
                                boe_EndDate
                            });

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay);
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        row.AddRange(travelPart1);

                        emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay); 
                        for (int i = 0; i < emptyCellsToAdd; i++)
                        {
                            row.Add(this.sEmpty);
                        }

                        TripDTO thisTrip = allTravelTrips.First(i => i.TripID == travelTrip.SystemTripID);
                        LocationDTO departureLocation = allDepartures.First(i => i.Id == thisTrip.DepartureLocationID);
                        LocationDTO destinationLocation = allDestinations.First(i => i.Id == thisTrip.DestinationLocationID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == thisTrip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == thisTrip.PerDiemID);

                        row.AddRange(
                            new string[] {
                            this.sEmpty,
                            this.sEmpty,
                            "Travel",
                            this.sEmpty,
                            this.sEmpty, this.sEmpty,
                            this.sEmpty,
                            perfOrgsFromDb.First(x => x.Id == travelTrip.PerfOrgID).PerformingOrgName,
                            perfOrgsFromDb.First(x => x.Id == travelTrip.PerfOrgID).PerformingOrgDesc,
                            allMiscTravelRates.First(x => x.Id == thisTrip.MiscTravelRateID).MiscTravelRateMode,
                            departureLocation.LocationName,
                            destinationLocation.LocationName,
                            travelTrip.TripDate.ToString("MM/yyyy"),
                            travelTrip.TripDate.ToString("MM/yyyy"),
                            this.sEmpty,
                            this.sEmpty,
                            this.sEmpty,
                            CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, this.travelTripCostCalculation.CalculateTravelCost(travelTrip, exportInputs.FullWorkspace, thisTrip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal)
                        });

                        foreach (CustomFieldDTO taskCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
                        {
                            ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == taskCustomField.Id).ToCollection();
                            CustomFieldValueDTO customFieldValueForTask = customFieldValues.FirstOrDefault(c => travelTrip.CustomFieldValueContainers.Select(t => t.CustomFieldValueID).Contains(c.CustomFieldValueID));
                            row.Add(customFieldValueForTask != null ? customFieldValueForTask.CustomFieldValueDescription : this.sEmpty);
                        }
                        toReturn.Add(row);
                    }
                }
                #endregion Travel

            }

            return toReturn;
        }

        /// <summary>
        /// Gets the boe status sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        private ExcelExportWorksheet GetBOEStatusSheetExportData(BOEExportInputs exportInputs)
        {
            Collection<BOEStatusReportModelView> statusReport = this.boeStatusReport.GenerateBOEStatusReport(exportInputs);

            // Reuse BOE Status Report Exporter since formats are the same
            ExcelExportWorksheet toReturn = this.boeStatusReport.GetExcelExportWorksheet(statusReport, (int)Reports.BOEStatusByBOE, exportInputs);
            toReturn.WorksheetName = "BOE Status";

            return toReturn;
        }

        /// <summary>
        /// Gets the resource spreads sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private ExcelExportWorksheet GetResourceSpreadsSheetExportData(BOEExportInputs exportInputs)
        {
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet("Resource Spreads");

            IReadOnlyCollection<BoeDTO> boes = exportInputs.Boes;

            // need workspace resources in case there are travel trips
            IReadOnlyCollection<ResourceDTO> workspaceResources = exportInputs.ResourcesUsedInWsBoes;
            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(exportInputs.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));
            HashSet<BoeTaskElementDTO> allTaskElements = new HashSet<BoeTaskElementDTO>(exportInputs.TaskElements);
            HashSet<OtherDirectCostDTO> allOdcs = new HashSet<OtherDirectCostDTO>(exportInputs.Odcs);
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(exportInputs.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(exportInputs.WbsElements);
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(exportInputs.Travels);
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(exportInputs.TravelTrips);
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(exportInputs.PerDiemsForTravelTrips);
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(exportInputs.EscalationRates);
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(exportInputs.MiscTravelRatesForTravelTrips);


            foreach (BoeDTO boe in boes)
            {
                ICollection<BoeTaskElementDTO> laborTasks = (allTaskElements.Where(m => m.BoeID == boe.Id && (m.TaskElementType == TaskElementType.Labor))).ToList();
                WbsDTO wbs = allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
                ClinDTO clin = allClins.FirstOrDefault(i => i.Id == boe.CLINID);

                foreach (BoeTaskElementDTO task in laborTasks)
                {
                    // Get the task ID
                    string taskID = task.BOETaskID;

                    foreach (ResourceTypeDto type in task.taskElementLabors.Where(l => l.StartDate.HasValue && l.EndDate.HasValue))
                    {
                        bool resourceUsesCostValues = type.SpreadType == SpreadType.Cost;

                        DateTime currentDate = type.StartDate.Value;

                        string wbsNumber, wbsTitle, clinNumber, clinTitle;

                        //Use resource level WBS/CLIN if BOE is set to Multi, otherwise display BOE's WBS/CLIN
                        if (boe.IsMultiClinWbs)
                        {
                            WbsDTO resourceWbs = type.WBSID != null ? allWbs.FirstOrDefault(w => w.Id == type.WBSID) : null;
                            ClinDTO resourceClin = type.CLINID != null ? allClins.FirstOrDefault(c => c.Id == type.CLINID) : null;

                            wbsNumber = resourceWbs != null ? resourceWbs.WbsNumber : this.sEmpty;
                            wbsTitle = resourceWbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceWbs.WbsTitle : this.sEmpty;
                            clinNumber = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinNumber : this.sEmpty;
                            clinTitle = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinTitle : this.sEmpty;
                        }
                        else
                        {
                            wbsNumber = wbs != null ? wbs.WbsNumber : this.sEmpty;
                            wbsTitle = wbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + wbs.WbsTitle : this.sEmpty;
                            clinNumber = clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinNumber : this.sEmpty;
                            clinTitle = clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinTitle : this.sEmpty;
                        }

                        while (currentDate <= type.EndDate.Value)
                        {
                            ResourceSpreadDto spread = (from s in type.LaborSpreads
                                          where s.LaborSpreadDate.Month == currentDate.Month &&
                                                s.LaborSpreadDate.Year == currentDate.Year
                                          select s).FirstOrDefault();

                            List<string> row = new List<string>();

                            ResourceDTO aResource = type.ResourceID.HasValue ? workspaceResources.First(x => x.Id == type.ResourceID.Value) : new ResourceDTO();

                            row.AddRange(
                                new string[]
                                {
                                    taskID,
                                    task.TaskTitle,
                                    task.MOQType.ToString(),
                                    wbsTitle,
                                    wbsNumber,
                                    clinNumber,
                                    clinTitle,
                                    type.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty,
                                    type.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                                    type.PerformingOrgID.HasValue ? perfOrgsFromDb.First(x => x.Id == type.PerformingOrgID.Value).PerformingOrgName : this.sEmpty,
                                    currentDate.Year.ToString(),
                                    currentDate.Month.ToString()
                                });

                            if (resourceUsesCostValues)
                            {
                                row.Add(string.Empty);  // Hours column is empty
                                if (spread == null)
                                {
                                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + "$0");
                                }
                                else
                                {
                                    decimal discreteCost = spread.LaborSpreadValue;
                                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + ((discreteCost == 0m) ? "$0" : "$" + discreteCost.ToString(Constants.NUMBER_WITH_COMMAS_FORMATTING)));
                                }
                            }
                            else  // hours
                            {
                                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + ((spread == null) ? "0" : spread.LaborSpreadValue.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision))));
                                row.Add(string.Empty);  // Cost column is empty
                            }

                            toReturn.Add(row);

                            currentDate = currentDate.AddMonths(1);
                        }
                    }
                }

                ICollection<OtherDirectCostDTO> odcTasks = allOdcs.Where(i => i.BoeID == boe.Id).ToCollection();

                HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this.resourceDTODataLoader.GetByIds(odcTasks.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
                HashSet<PerformingOrgDTO> perfOrgsFromDbForOdc = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(odcTasks.SelectMany(x => x.ODCTypes).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

                foreach (OtherDirectCostDTO task in odcTasks)
                {
                    foreach (OtherDirectCostType type in task.ODCTypes)
                    {
                        DateTime currentDate = type.StartDate.Value;

                        while (currentDate <= type.EndDate.Value)
                        {
                            OtherDirectCostSpread spread = (from s in type.ODCSpreads
                                          where s.ODCSpreadDate.HasValue &&
                                                s.ODCSpreadDate.Value.Month == currentDate.Month &&
                                                s.ODCSpreadDate.Value.Year == currentDate.Year
                                          select s).FirstOrDefault();

                            List<string> row = new List<string>();

                            ResourceDTO aResource = type.ResourceID.HasValue ? resourcesFromDb.First(x => x.Id == type.ResourceID.Value) : new ResourceDTO();

                            row.AddRange(
                                new string[]
                                {
                                    task.Id.ToString(),
                                    task.TaskTitle,
                                    this.sEmpty,
                                    wbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + wbs.WbsTitle : this.sEmpty,
                                    wbs != null ? wbs.WbsNumber : this.sEmpty,
                                    clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinNumber : this.sEmpty,
                                    clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinTitle : this.sEmpty,
                                    type.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty,
                                    type.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                                    type.PerformingOrgID.HasValue ? perfOrgsFromDbForOdc.First(x => x.Id == type.PerformingOrgID.Value).PerformingOrgName : this.sEmpty,
                                    currentDate.Year.ToString(),
                                    currentDate.Month.ToString(),
                                    this.sEmpty
                                });

                            if (spread == null)
                            {
                                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + "$0");
                            }
                            else
                            {
                                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + (spread.CostSpreadValue.HasValue ? string.Format(Constants.MONEY_FORMATTING, ((decimal)(spread.CostSpreadValue.Value) / 100)) : this.sEmpty));
                            }

                            toReturn.Add(row);

                            currentDate = currentDate.AddMonths(1);
                        }
                    }
                }              

                ICollection<TravelDTO> travelTasks = allTravels.Where(i => i.BoeID == boe.Id).ToCollection();
                HashSet<PerformingOrgDTO> perfOrgsFromDbForTravel = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(travelTasks.SelectMany(x => x.TravelTrips).Select(x => x.PerfOrgID).Distinct().ToList()));

                foreach (TravelDTO task in travelTasks)
                {
                    foreach (TravelTripType type in task.TravelTrips)
                    {
                        List<string> row = new List<string>();
                        TripDTO trip = allTravelTrips.First(i => i.TripID == type.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);

                        ResourceDTO travelTripWithResource = (from r in workspaceResources
                                                      where r.Segment == type.Segment && r.ElementOfCost == ElementOfCostType.Travel
                                                      select r).FirstOrDefault(); // only travel resource can exist for each segment so don't have to worry about getting more than 1
                        row.AddRange(
                            new string[]
                                {
                                    task.TaskID,
                                    task.TaskTitle,
                                    this.sEmpty,
                                    wbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + wbs.WbsTitle : this.sEmpty,
                                    wbs != null ? wbs.WbsNumber : this.sEmpty,
                                    clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinNumber : this.sEmpty,
                                    clin != null ? CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinTitle : this.sEmpty,
                                    travelTripWithResource != null ? travelTripWithResource.SegRegion : this.sEmpty,
                                    travelTripWithResource != null ? travelTripWithResource.LaborType : this.sEmpty,
                                    perfOrgsFromDbForTravel.First(x => x.Id == type.PerfOrgID).PerformingOrgName ,
                                    type.TripDate.Year.ToString(),
                                    type.TripDate.Month.ToString(),
                                    this.sEmpty,// empty for hours column
                                    CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, (this.travelTripCostCalculation.CalculateTravelCost(type, exportInputs.FullWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal))
                                });

                        toReturn.Add(row);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the boe resource combo sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="metricNameTaskElementMappingDTO">The metric name task element mapping dto.</param>
        /// <returns>Worksheet with data for BOE & Resource Combo tab of the Workspace Data Report</returns>
        private ExcelExportWorksheet GetBOEResourceComboSheetExportData(BOEExportInputs exportInputs, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO)
        {
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet("BOE & Resource Combo");

            IReadOnlyCollection<BoeDTO> boes = exportInputs.Boes.ToCollection(); 

            // Bulk load all data.
            IReadOnlyCollection<CustomFieldDTO> workspace_customFields = exportInputs.CustomFields;
            ICollection<CustomFieldValueDTO> workspaceCustomFieldValues = new List<CustomFieldValueDTO>();
            if (workspace_customFields.Any())
            {
                workspaceCustomFieldValues = this.GetWorkspaceCustomFieldValues(workspace_customFields);
            }

            IReadOnlyCollection<ResourceDTO> workspaceResources = exportInputs.ResourcesUsedInWsBoes;
            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(exportInputs.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));
            HashSet<BoeTaskElementDTO> allTaskElements = new HashSet<BoeTaskElementDTO>(exportInputs.TaskElements);
            HashSet<OtherDirectCostDTO> allOdcs = new HashSet<OtherDirectCostDTO>(exportInputs.Odcs);
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(exportInputs.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(exportInputs.WbsElements);
            IDictionary<int, SpreadCurveModelView> allSpreadCurves = this.CommonDataMapper.getSpreadCurveDictionary();
            IDictionary<int, OtherDirectCostSpreadCurveModelView> allOdcSpreadCurves = this.CommonDataMapper.getOdcSpreadCurveDictionary();

            toReturn = this.GetBOEDataforBOEResourceCombo(exportInputs, toReturn, boes, allTaskElements, allOdcs, allWbs, allClins, workspace_customFields, workspaceCustomFieldValues, metricNameTaskElementMappingDTO, workspaceResources, allSpreadCurves, allOdcSpreadCurves, perfOrgsFromDb);
            
            return toReturn;
        }

        /// <summary>
        /// Gets the BOE row data for the BOE Resource Combo sheet
        /// Makes call to methods to get task data
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boes">BOEs in the workspace</param>
        /// <param name="allTaskElements">All Task Elements</param>
        /// <param name="allOdcs">All ODCs</param>
        /// <param name="allWbs">All WBSs</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        /// <param name="metricNameTaskElementMappingDTO">Task element to historical metric mapping.</param>
        /// <param name="workspaceResources">Workspace Resources</param>
        /// <param name="allSpreadCurves">All Labor Spread Curves</param>
        /// <param name="allOdcSpreadCurves">All ODC Spread Curves</param>
        /// <param name="perfOrgsFromDb">Performing Orgs</param>
        /// <returns>
        /// Excel Export Worksheet with BOE data
        /// </returns>
        private ExcelExportWorksheet GetBOEDataforBOEResourceCombo(BOEExportInputs exportInputs, ExcelExportWorksheet toReturn, IReadOnlyCollection<BoeDTO> boes, HashSet<BoeTaskElementDTO> allTaskElements, HashSet<OtherDirectCostDTO> allOdcs, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO, IReadOnlyCollection<ResourceDTO> workspaceResources, IDictionary<int, SpreadCurveModelView> allSpreadCurves, IDictionary<int, OtherDirectCostSpreadCurveModelView> allOdcSpreadCurves, HashSet<PerformingOrgDTO> perfOrgsFromDb)
        {
            foreach (BoeDTO boe in boes)
            {
                ICollection<BoeTaskElementDTO> laborTasks = this.GetLaborTasksForBoe(boe, allTaskElements);
                ICollection<OtherDirectCostDTO> ODCs = this.GetODCsForBoe(boe, allOdcs);
                
                List<string> row = new List<string>();

                row.AddRange(
                    new string[]{
                        boe.Id.ToString(),
                        boe.isMaterial ?  "Material BOE": "BOE",
                        this.sEmpty, // Task ID
                        this.sEmpty, // Task Title
                        this.sEmpty, // MOQ Equation
                        this.sEmpty, // MOQ Type
                        this.sEmpty, // MOQ Rationale
                        this.sEmpty // Segment Region
                    });

                row.AddRange(this.GetBoeWbsClinTitleAndDateFields(boe, allWbs, allClins));

                this.GetBoeCustomFields(row, boe, workspace_customFields, workspaceCustomFieldValues);

                this.GetBoeHoursAndCost(row, laborTasks, ODCs, exportInputs);

                row.Add(boe.isMaterial ? this.sYes : this.sNo);

                toReturn.Add(row);

                // Labor Tasks
                toReturn = this.GetLaborTaskDataforBOEResourceCombo(exportInputs, toReturn, boe, laborTasks, workspace_customFields, workspaceCustomFieldValues, metricNameTaskElementMappingDTO, allWbs, allClins, workspaceResources, allSpreadCurves, perfOrgsFromDb);

                // ODC Tasks
                toReturn = this.GetODCTaskDataforBOEResourceCombo(toReturn, boe, ODCs, allWbs, allClins, workspace_customFields, allOdcSpreadCurves);                
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the Labor Task row data for the BOE Resource Combo sheet
        /// Makes call to method to get Resource Type data
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="laborTasks">Labor Tasks</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        /// <param name="metricNameTaskElementMappingDTO">Task element to historical metric mapping.</param>
        /// <param name="allWbs">All WBSs</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="workspaceResources">Workspace Resources</param>
        /// <param name="allSpreadCurves">All Labor Spread Curves</param>
        /// <param name="perfOrgsFromDb">Performing Orgs</param>
        /// <returns>
        /// Excel Export Worksheet with Labor Task data
        /// </returns>
        private ExcelExportWorksheet GetLaborTaskDataforBOEResourceCombo(BOEExportInputs exportInputs, ExcelExportWorksheet toReturn, BoeDTO boe, ICollection<BoeTaskElementDTO> laborTasks, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, IReadOnlyCollection<ResourceDTO> workspaceResources, IDictionary<int, SpreadCurveModelView> allSpreadCurves, HashSet<PerformingOrgDTO> perfOrgsFromDb)
        {
            foreach (BoeTaskElementDTO task in laborTasks)
            {
                List<string> row = new List<string>();
                
                row.AddRange(
                    new string[] {
                            boe.Id.ToString(),
                            "Task"
                    });

                string[] taskFields1 = this.GetTaskIdTitleAndMOQFields(exportInputs, task);
                row.AddRange(taskFields1);
                row.Add(this.sEmpty); // Segment Region
                row.AddRange(this.GetBoeWbsClinTitleAndDateFields(boe, allWbs, allClins));

                // Add blanks for BOE-level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                string[] taskFields2 = this.GetTaskDescriptionMetricsAndDateFields(boe, task, metricNameTaskElementMappingDTO, exportInputs);
                row.AddRange(taskFields2);

                this.GetTaskCustomFields(row, task, workspace_customFields, workspaceCustomFieldValues);

                this.GetTaskHoursAndCost(row, task);

                toReturn.Add(row);

                // Labor Resource Types
                toReturn = this.GetLaborResourceTypeDataforBOEResourceCombo(exportInputs, toReturn, boe, task, allWbs, allClins, workspaceResources, workspace_customFields, workspaceCustomFieldValues, allSpreadCurves, perfOrgsFromDb, taskFields1, taskFields2);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the Labor Resource Type row data for the BOE Resource Combo sheet
        /// Makes call to method to get Resource Spread data
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="task">Labor Task</param>
        /// <param name="allWbs">All WBSs</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="workspaceResources">Workspace Resources</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        /// <param name="allSpreadCurves">All Labor Spread Curves</param>
        /// <param name="perfOrgsFromDb">Performing Orgs</param>
        /// <param name="taskFields1">task field data from GetTaskIdTitleAndMOQFields for the row</param>
        /// <param name="taskFields2">task field data from GetTaskDescriptionMetricsAndDateFields for the row</param>
        /// <returns>
        /// Excel Export Worksheet with Labor Task data
        /// </returns>
        private ExcelExportWorksheet GetLaborResourceTypeDataforBOEResourceCombo(BOEExportInputs exportInputs, ExcelExportWorksheet toReturn, BoeDTO boe, BoeTaskElementDTO task, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, IReadOnlyCollection<ResourceDTO> workspaceResources, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, IDictionary<int, SpreadCurveModelView> allSpreadCurves, HashSet<PerformingOrgDTO> perfOrgsFromDb, string[] taskFields1, string[] taskFields2)
        {
            foreach (ResourceTypeDto resourceType in task.taskElementLabors)
            {
                List<string> row = new List<string>();
                bool resourceUsesCostValues = resourceType.SpreadType == SpreadType.Cost;
                DateTime currentDate = resourceType.StartDate.Value;                
                ResourceDTO aResource = resourceType.ResourceID.HasValue ? workspaceResources.First(x => x.Id == resourceType.ResourceID.Value) : new ResourceDTO();

                row.AddRange(
                    new string[]
                    {
                        boe.Id.ToString(),
                        "Resource Type"
                    });

                row.AddRange(taskFields1);
                string[] resFields1 = this.GetResourceWbsClinAndBoeFields(resourceType, aResource, allWbs, allClins, boe);
                row.AddRange(resFields1);

                // Empty cells for BOE level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                row.AddRange(taskFields2);

                // Empty cells for Task level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.TaskDisplay);

                string[] resFields2 = this.GetResourceTypeDetails(resourceType, aResource, task, perfOrgsFromDb, allSpreadCurves, exportInputs);
                row.AddRange(resFields2);

                row.AddRange(this.GetResourceTypeHourAndCostSpreads(resourceType, exportInputs));

                this.GetResourceCustomFields(row, resourceType, workspace_customFields, workspaceCustomFieldValues);

                toReturn.Add(row);

                // Labor Resource Spreads
                toReturn = this.GetLaborSpreadDataforBOEResourceCombo(exportInputs, toReturn, boe, resourceType, taskFields1, taskFields2, resFields1, resFields2, currentDate, workspace_customFields, workspaceCustomFieldValues, resourceUsesCostValues);
                
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the Labor Resource Spread row data for the BOE Resource Combo sheet
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="resourceType">Resource Type</param>
        /// <param name="taskFields1">task field data from GetTaskIdTitleAndMOQFields for the row</param>
        /// <param name="taskFields2">task field data from GetTaskDescriptionMetricsAndDateFields for the row</param>
        /// <param name="resFields1">resource type field data for the row (part 1)</param>
        /// <param name="resFields2">resource type field data for the row (part 2)</param>
        /// <param name="currentDate">Resource Start Date - used to increment spread dates</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        /// <param name="resourceUsesCostValues">Bool noting if Resource Type uses a Cost Spread</param>
        /// <returns>
        /// Excel Export Worksheet with Labor Task data
        /// </returns>
        private ExcelExportWorksheet GetLaborSpreadDataforBOEResourceCombo(BOEExportInputs exportInputs, ExcelExportWorksheet toReturn, BoeDTO boe, ResourceTypeDto resourceType, string[] taskFields1, string[] taskFields2, string[] resFields1, string[] resFields2, DateTime currentDate, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, bool resourceUsesCostValues)
        {
            while (currentDate <= resourceType.EndDate.Value)
            {
                List<string> row = new List<string>();

                ResourceSpreadDto spread = (from s in resourceType.LaborSpreads
                                            where s.LaborSpreadDate.Month == currentDate.Month &&
                                                  s.LaborSpreadDate.Year == currentDate.Year
                                            select s).FirstOrDefault();

                row.AddRange(
                new string[]
                {
                                    boe.Id.ToString(),
                                    "Resource Spread"
                });

                row.AddRange(taskFields1);
                row.AddRange(resFields1);

                // Empty cells for BOE level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                row.AddRange(taskFields2);

                // Empty cells for Task level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.TaskDisplay);

                row.AddRange(resFields2);

                row.AddRange(new string[]
                {
                                this.sEmpty, //Hours Spread
                                this.sEmpty // Cost Spread
                });

                this.GetResourceCustomFields(row, resourceType, workspace_customFields, workspaceCustomFieldValues);

                row.AddRange(
                    new string[]
                    {
                                    currentDate.Year.ToString(),
                                    currentDate.Month.ToString()
                    });

                if (resourceUsesCostValues)
                {
                    row.Add(string.Empty);  // Hours column is empty
                    if (spread == null)
                    {
                        row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + "$0");
                    }
                    else
                    {
                        decimal discreteCost = spread.LaborSpreadValue;
                        row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + ((discreteCost == 0m) ? "$0" : "$" + discreteCost.ToString(Constants.NUMBER_WITH_COMMAS_FORMATTING)));
                    }
                }
                else  // hours
                {
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + ((spread == null) ? "0" : spread.LaborSpreadValue.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision))));
                    row.Add(string.Empty);  // Cost column is empty
                }

                toReturn.Add(row);

                currentDate = currentDate.AddMonths(1);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the ODC Task row data for the BOE Resource Combo sheet
        /// Makes call to method to get Resource Type data
        /// </summary>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="ODCs">ODC Tasks</param>
        /// <param name="allWbs">All WBS</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="allOdcSpreadCurves">All ODC Spread Curves</param>
        /// <returns>Excel Export Worksheet with ODC Task data</returns>
        private ExcelExportWorksheet GetODCTaskDataforBOEResourceCombo(ExcelExportWorksheet toReturn, BoeDTO boe, ICollection<OtherDirectCostDTO> ODCs, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, IDictionary<int, OtherDirectCostSpreadCurveModelView> allOdcSpreadCurves)
        {
            foreach (OtherDirectCostDTO odc in ODCs)
            {
                List<string> row = new List<string>();

                ICollection<string> plainText = RTEUtilities.TurnHTMLIntoPlainText(new List<string>() { odc.TaskDescription, odc.MoqText });
                string taskDescription = plainText.ElementAt(0);
                string MOQText = plainText.ElementAt(1);

                row.AddRange(
                    new string[] {
                            boe.Id.ToString(),
                            "ODC Task",
                            odc.Id.ToString(),
                            odc.TaskTitle,
                            this.sEmpty, // MOQ Equation
                            this.sEmpty, // MOQ Type
                            MOQText,
                            this.sEmpty // Segment Region
                    });

                row.AddRange(this.GetBoeWbsClinTitleAndDateFields(boe, allWbs, allClins));

                // Add blanks for BOE-level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                // Create variable for reuse in resource row
                string[] taskfields = new string[]
                {
                        this.sEmpty, // Total BOE Hours
                        this.sEmpty, // Total BOE Cost
                        boe.isMaterial ? this.sYes : this.sNo,
                        taskDescription,
                        this.sEmpty,
                        odc.StartDate.HasValue ? odc.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                        odc.EndDate.HasValue ? odc.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                };
                row.AddRange(taskfields);

                // empty cells for task-level CFs (no CFs in ODC tasks)
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.TaskDisplay);

                row.Add(this.sEmpty); // Hours - ODC is cost only
                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, ((decimal)(odc.ODCTypes.Sum(odct => odct.ODCSpreads.Sum(odcs => odcs.CostSpreadValue))) / 100)));

                toReturn.Add(row);

                HashSet<ResourceDTO> odcResourcesFromDb = this.GetOdcResourcesFromDb(odc);
                HashSet<PerformingOrgDTO> odcPerfOrgsFromDb = this.GetOdcPerfOrgsFromDb(odc);

                toReturn = this.GetODCResourceTypeDataforBOEResourceCombo(toReturn, boe, odc, odcResourcesFromDb, odcPerfOrgsFromDb, MOQText, allWbs, allClins, workspace_customFields, taskfields, allOdcSpreadCurves);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the ODC Resource Type row data for the BOE Resource Combo sheet
        /// Makes call to method to get Resource Spread data
        /// </summary>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="odc">ODC Task</param>
        /// <param name="odcResourcesFromDb">ODC Resources</param>
        /// <param name="odcPerfOrgsFromDb">ODC Performing Orgs</param>
        /// <param name="MOQText">MOQ Text</param>
        /// <param name="allWbs">All WBS</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="taskfields">Task field data for row</param>
        /// <param name="allOdcSpreadCurves">All ODC Spread Curves</param>
        /// <returns>Excel Export Worksheet with ODC Task data</returns>
        private ExcelExportWorksheet GetODCResourceTypeDataforBOEResourceCombo(ExcelExportWorksheet toReturn, BoeDTO boe, OtherDirectCostDTO odc, HashSet<ResourceDTO> odcResourcesFromDb, HashSet<PerformingOrgDTO> odcPerfOrgsFromDb, string MOQText, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, string[] taskfields, IDictionary<int, OtherDirectCostSpreadCurveModelView> allOdcSpreadCurves)
        {
            foreach (OtherDirectCostType odcType in odc.ODCTypes)
            {
                List<string> row = new List<string>();

                DateTime currentDate = odcType.StartDate.Value;

                ResourceDTO aResource = odcType.ResourceID.HasValue ? odcResourcesFromDb.First(x => x.Id == odcType.ResourceID.Value) : new ResourceDTO();

                row.AddRange(
                    new string[]
                    {
                                    boe.Id.ToString(),
                                    "ODC Type"
                    });

                string[] odcFields1 = new string[]
                    {
                                    odc.Id.ToString(),
                                    odc.TaskTitle,
                                    this.sEmpty, // MOQ Equation
                                    this.sEmpty, // MOQ Type
                                    MOQText,
                                    odcType.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty
                    };
                row.AddRange(odcFields1);

                row.AddRange(this.GetBoeWbsClinTitleAndDateFields(boe, allWbs, allClins));

                // Empty cells for BOE level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                row.AddRange(taskfields);

                // Empty cells for Task level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.TaskDisplay);

                PerformingOrgDTO perfOrg = odcType.PerformingOrgID.HasValue ? odcPerfOrgsFromDb.First(x => x.Id == odcType.PerformingOrgID.Value) : new PerformingOrgDTO();
                OtherDirectCostSpreadCurveModelView spreadCurve;
                string spreadCurveName = odcType.SpreadCurve.ToString();
                if (allOdcSpreadCurves.TryGetValue((int)odcType.SpreadCurve, out spreadCurve))
                {
                    spreadCurveName = spreadCurve.SpreadCurveName;
                }

                string[] odcFields2 = new string[]
                    {
                                    this.sEmpty, // Total Task Hours
                                    this.sEmpty, // Total Task Cost
                                    odcType.ResourceID.HasValue ? aResource.ElementOfCost.ToString() : this.sEmpty,
                                    odcType.ResourceID.HasValue ? aResource.ResourceDesc : this.sEmpty,
                                    odcType.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                                    odcType.ResourceID.HasValue ? aResource.ResourceName : this.sEmpty,
                                    odcType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgName : this.sEmpty,
                                    odcType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgDesc : this.sEmpty,
                                    spreadCurveName,
                                    this.sEmpty, // percent spread
                                    this.sEmpty // Hours
                    };
                row.AddRange(odcFields2);

                row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, (((decimal)odcType.ODCSpreads.Sum(odcs2 => odcs2.CostSpreadValue)) / 100)));

                toReturn.Add(row);

                toReturn = this.GetODCResourceSpreadDataforBOEResourceCombo(toReturn, boe, odcType, currentDate, taskfields, odcFields1, odcFields2, workspace_customFields, allWbs, allClins);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the ODC Resource Spread row data for the BOE Resource Combo sheet
        /// </summary>
        /// <param name="toReturn">Excel Export Worksheet to add the rows to</param>
        /// <param name="boe">BOE</param>
        /// <param name="odcType">ODC Resource Type</param>
        /// <param name="currentDate">Resource Start Date - used to increment spread dates</param>
        /// <param name="taskfields">Task field data for row</param>
        /// <param name="odcFields1">odc resource field data for row (part 1)</param>
        /// <param name="odcFields2">odc resource field data for row (part 2)</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="allWbs">All WBS</param>
        /// <param name="allClins">All CLINs</param>
        /// <returns>Excel Export Worksheet with ODC Task data</returns>
        private ExcelExportWorksheet GetODCResourceSpreadDataforBOEResourceCombo(ExcelExportWorksheet toReturn, BoeDTO boe, OtherDirectCostType odcType, DateTime currentDate, string[] taskfields, string[] odcFields1, string[] odcFields2, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins)
        {
            while (currentDate <= odcType.EndDate.Value)
            {
                List<string> row = new List<string>();

                row.AddRange(
                    new string[]
                    {
                                    boe.Id.ToString(),
                                    "ODC Spread"
                    });

                row.AddRange(odcFields1);
                row.AddRange(this.GetBoeWbsClinTitleAndDateFields(boe, allWbs, allClins));

                // Empty cells for BOE level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.BoeDisplay);

                row.AddRange(taskfields);

                // Empty cells for Task level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.TaskDisplay);

                row.AddRange(odcFields2);

                row.Add(this.sEmpty); // Cost Spread

                // Empty cells for Resource level CFs
                this.AddBlankCustomFieldCells(row, workspace_customFields, CustomFieldType.LaborTypeDisplay);

                row.AddRange(
                    new string[]
                    {
                                    currentDate.Year.ToString(),
                                    currentDate.Month.ToString(),
                                    this.sEmpty // Hours
                    });

                OtherDirectCostSpread spread = (from s in odcType.ODCSpreads
                                                where s.ODCSpreadDate.HasValue &&
                                                      s.ODCSpreadDate.Value.Month == currentDate.Month &&
                                                      s.ODCSpreadDate.Value.Year == currentDate.Year
                                                select s).FirstOrDefault();

                if (spread == null)
                {
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + "$0");
                }
                else
                {
                    row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + (spread.CostSpreadValue.HasValue ? string.Format(Constants.MONEY_FORMATTING, ((decimal)(spread.CostSpreadValue.Value) / 100)) : this.sEmpty));
                }

                toReturn.Add(row);

                currentDate = currentDate.AddMonths(1);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the Workspace Custom Field Values
        /// </summary>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <returns>Workspace Custom Field Values</returns>
        private ICollection<CustomFieldValueDTO> GetWorkspaceCustomFieldValues(IReadOnlyCollection<CustomFieldDTO> workspace_customFields)
        {
            return this.customFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(workspace_customFields.Select(i => i.Id).ToList());
        }

        /// <summary>
        /// Gets the BOE's Labor Tasks
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="allTaskElements">All Task Elements</param>
        /// <returns>Labor Tasks for the BOE</returns>
        private ICollection<BoeTaskElementDTO> GetLaborTasksForBoe(BoeDTO boe, HashSet<BoeTaskElementDTO> allTaskElements)
        {
            return (allTaskElements.Where(m => m.BoeID == boe.Id && (m.TaskElementType == TaskElementType.Labor))).ToList();
        }

        /// <summary>
        /// Gets the BOE's ODC Tasks
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="allOdcs">All ODCs</param>
        /// <returns>ODC Tasks for the BOE</returns>
        private ICollection<OtherDirectCostDTO> GetODCsForBoe(BoeDTO boe, HashSet<OtherDirectCostDTO> allOdcs)
        {
            return allOdcs.Where(i => i.BoeID == boe.Id).ToCollection();
        }

        /// <summary>
        /// Gets the BOE WBS, BOE CLIN, IsMulti yes/no, BOE Title, and BOE Start and End Date for the BOE & Resource Combo Sheet
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="allWbs">All WBS</param>
        /// <param name="allClins">All CLIN</param>
        /// <returns>Strings for the BOE WBS, CLIN, Title, and Date fields</returns>
        private string[] GetBoeWbsClinTitleAndDateFields(BoeDTO boe, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins)
        {
            WbsDTO boe_WBS = allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
            ClinDTO boe_CLIN = allClins.FirstOrDefault(i => i.Id == boe.CLINID);

            String boe_StartDate = boe.StartDate.ToString("MM/yyyy");
            String boe_EndDate = boe.EndDate.ToString("MM/yyyy");

            return new string[] {
                        boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty,
                        boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty,
                        boe.IsMultiClinWbs ? this.sYes : this.sNo,
                        boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty,
                        boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty,
                        boe.Title != null ? boe.Title : this.sEmpty,
                        boe_StartDate,
                        boe_EndDate};
        }

        /// <summary>
        /// Gets the BOE-level custom fields
        /// </summary>
        /// <param name="row">Row to add the custom fields to</param>
        /// <param name="boe">BOE</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        private void GetBoeCustomFields(List<string> row, BoeDTO boe, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues)
        {
            foreach (CustomFieldDTO boeCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.BoeDisplay))
            {
                ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == boeCustomField.Id).ToCollection();
                CustomFieldValueDTO customFieldValueForBOE = customFieldValues.FirstOrDefault(c => boe.CustomFieldValueContainers.Select(b => b.CustomFieldValueID).Contains(c.CustomFieldValueID));
                row.Add(customFieldValueForBOE != null ? customFieldValueForBOE.CustomFieldValueDescription : this.sEmpty);
            }
        }

        /// <summary>
        /// Gets the boe hours and cost.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="laborTasks">The labor tasks.</param>
        /// <param name="ODCs">The od cs.</param>
        /// <param name="exportInputs">The export inputs.</param>
        private void GetBoeHoursAndCost(List<string> row, ICollection<BoeTaskElementDTO> laborTasks, ICollection<OtherDirectCostDTO> ODCs, BOEExportInputs exportInputs)
        {
            decimal boe_HoursSum = laborTasks.Sum(t => t.taskElementLabors.Where(l => l.SpreadType == SpreadType.Hours && l.ValueSpread.HasValue).Sum(l => l.ValueSpread.Value));

            decimal boe_CostSum =
                (decimal)(laborTasks.Sum(t => t.taskElementLabors.Where(l => l.SpreadType == SpreadType.Cost && l.ValueSpread.HasValue).Sum(l => l.ValueSpread.Value)))
                + (((decimal)(ODCs.Sum(odc => odc.ODCTypes.Sum(odcTypes => odcTypes.ODCSpreads.Sum(odcSpreads => odcSpreads.CostSpreadValue))))) / 100);

            row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe_HoursSum.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision)));
            row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe_CostSum));
        }

        /// <summary>
        /// Gets the task identifier title and moq fields.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="task">The task.</param>
        /// <param name="laborTasks">The labor tasks.</param>
        /// <returns></returns>
        private string[] GetTaskIdTitleAndMOQFields(BOEExportInputs exportInputs, BoeTaskElementDTO task)
        {
            // Get the task ID
            string taskID = task.BOETaskID;
            string taskMOQText = RTEUtilities.TurnHTMLIntoPlainText(BOEExportConverter.GetRteOverride(task.BoeID, task.Id, task.MOQText, RteTemplateSource.TaskMOQ, exportInputs.RTETemplatesOverrides));

            decimal isDecimal;

            // Remove comma from a decimal value otherwise Excel will generate a Numbers stored as text error when opening.
            // Ex: 1,000 will be exported as 1000
            if (!string.IsNullOrEmpty(task.MOQHoursEquation) && decimal.TryParse(task.MOQHoursEquation, out isDecimal))
            {
                task.MOQHoursEquation = isDecimal.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision));
            }

            return new string[]
            {
                taskID,
                task.TaskTitle,
                (task.TaskElementType == TaskElementType.Labor)
                    ? Parser.UntagVariables(task.MOQHoursEquation, exportInputs.WorkspaceVariables.ToList())
                    : this.sEmpty,
                task.MOQType.GetDescription(),
                taskMOQText
            };
        }

        /// <summary>
        /// Adds blank cells for the number of custom fields of the given type
        /// </summary>
        /// <param name="row">Row to add blank cells to</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="customFieldType">Type of Custom Field to add blanks for</param>
        private void AddBlankCustomFieldCells(List<string> row, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, CustomFieldType customFieldType)
        {
            int emptyCellsToAdd = workspace_customFields.Count(c => c.CustomFieldDisplayID == customFieldType);
            for (int i = 0; i < emptyCellsToAdd; i++)
            {
                row.Add(this.sEmpty);
            }
        }

        /// <summary>
        /// Gets the Task Description and dates, Historical Metric string, and Material yes/no for the BOE & Resource Combo
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="task">Task</param>
        /// <param name="metricNameTaskElementMappingDTO">Metric Name Task Element Mapping DTO</param>
        /// <returns>strings for the Task description, metrics, and date fields</returns>
        private string[] GetTaskDescriptionMetricsAndDateFields(BoeDTO boe, BoeTaskElementDTO task, MetricNameTaskElementMappingDTO metricNameTaskElementMappingDTO, BOEExportInputs exportInputs)
        {
            string taskDescription = RTEUtilities.TurnHTMLIntoPlainText(BOEExportConverter.GetRteOverride(boe.Id, task.Id, task.Description, RteTemplateSource.BoeDescription, exportInputs.RTETemplatesOverrides));

            string historicalMetricString = metricNameTaskElementMappingDTO.GetMetricNamesByTaskElementId(task.Id);

            return new string[]
                {
                        this.sEmpty, // Total BOE Hours
                        this.sEmpty, // Total BOE Cost
                        boe.isMaterial ? this.sYes : this.sNo,
                        taskDescription,
                        historicalMetricString,
                        task.StartDate.HasValue ? task.StartDate.Value.ToString("MM/yyyy") : this.sEmpty,
                        task.EndDate.HasValue ? task.EndDate.Value.ToString("MM/yyyy") : this.sEmpty
                };
        }

        /// <summary>
        /// Gets the Task-Level Custom Fields
        /// </summary>
        /// <param name="row">row to add custom fields to</param>
        /// <param name="task">Task</param>
        /// <param name="workspace_customFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        private void GetTaskCustomFields(List<string> row, BoeTaskElementDTO task, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues)
        {
            foreach (CustomFieldDTO taskCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.TaskDisplay))
            {
                ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == taskCustomField.Id).ToCollection();
                CustomFieldValueDTO customFieldValueForTask = customFieldValues.FirstOrDefault(c => task.CustomFieldValueContainers.Select(t => t.CustomFieldValueID).Contains(c.CustomFieldValueID));
                row.Add(customFieldValueForTask != null ? customFieldValueForTask.CustomFieldValueDescription : this.sEmpty);
            }
        }

        /// <summary>
        /// Gets the Task Total Hours and Total Cost
        /// </summary>
        /// <param name="row">Row to add hours and cost to</param>
        /// <param name="task">Task</param>
        private void GetTaskHoursAndCost(List<string> row, BoeTaskElementDTO task)
        {
            row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + task.taskElementLabors.Where(l => l.SpreadType == SpreadType.Hours).Sum(l => l.ValueSpread).ToString());

            // Sum up all the Cost type resources.
            decimal taskElementCostTotal = task.taskElementLabors.Where(l => l.SpreadType == SpreadType.Cost).Sum(l => l.ValueSpread.HasValue ? l.ValueSpread.Value : 0);
            
            row.Add(CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, taskElementCostTotal));
        }

        /// <summary>
        /// Gets the Resource WBS, CLIN, mutliboe yes/no, BOE Title and dates
        /// </summary>
        /// <param name="resourceType">Resource Type</param>
        /// <param name="aResource">Resource</param>
        /// <param name="allWbs">All WBS</param>
        /// <param name="allClins">All CLINs</param>
        /// <param name="boe">BOE</param>
        /// <returns>strings for Resource WBS, CLIN, and BOE Fields</returns>
        private string[] GetResourceWbsClinAndBoeFields(ResourceTypeDto resourceType, ResourceDTO aResource, HashSet<WbsDTO> allWbs, HashSet<ClinDTO> allClins, BoeDTO boe)
        {
            WbsDTO boe_WBS = allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
            ClinDTO boe_CLIN = allClins.FirstOrDefault(i => i.Id == boe.CLINID);

            string res_WBSNumber, res_WBSTitle, res_CLINNumber, res_CLINTitle;

            //Use resource level WBS/CLIN if BOE is set to Multi, otherwise display BOE's WBS/CLIN
            if (boe.IsMultiClinWbs)
            {
                WbsDTO resourceWbs = resourceType.WBSID != null ? allWbs.FirstOrDefault(w => w.Id == resourceType.WBSID) : null;
                ClinDTO resourceClin = resourceType.CLINID != null ? allClins.FirstOrDefault(c => c.Id == resourceType.CLINID) : null;

                res_WBSNumber = resourceWbs != null ? resourceWbs.WbsNumber : this.sEmpty;
                res_WBSTitle = resourceWbs != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceWbs.WbsTitle : this.sEmpty;
                res_CLINNumber = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinNumber : this.sEmpty;
                res_CLINTitle = resourceClin != null ? CommonConstants.FORCE_AS_STRING_VALUE + resourceClin.ClinTitle : this.sEmpty;
            }
            else
            {
                res_WBSNumber = boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty;
                res_WBSTitle = boe_WBS != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_WBS.WbsTitle : this.sEmpty;
                res_CLINNumber = boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinNumber : this.sEmpty;
                res_CLINTitle = boe_CLIN != null ? CommonConstants.FORCE_AS_STRING_VALUE + boe_CLIN.ClinTitle : this.sEmpty;
            }

            String boe_StartDate = boe.StartDate.ToString("MM/yyyy");
            String boe_EndDate = boe.EndDate.ToString("MM/yyyy");

            return new string[]
                    {
                                    resourceType.ResourceID.HasValue ? aResource.SegRegion : this.sEmpty,
                                    res_WBSNumber,
                                    res_WBSTitle,
                                    boe.IsMultiClinWbs ? this.sYes : this.sNo,
                                    res_CLINNumber,
                                    res_CLINTitle,
                                    boe.Title,
                                    boe_StartDate,
                                    boe_EndDate
                    };
        }

        /// <summary>
        /// Gets the Element of Cost, Resource Description, Labor Type, Resource Name, Perf Org, Spread Curve, and Percent Spread fields
        /// </summary>
        /// <param name="resourceType">Resource Type</param>
        /// <param name="aResource">Resource</param>
        /// <param name="task">Task</param>
        /// <param name="perfOrgsFromDb">Perf Orgs</param>
        /// <param name="allSpreadCurves">All Spread Curves</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// Strings for the Resource Type fields
        /// </returns>
        private string[] GetResourceTypeDetails(ResourceTypeDto resourceType, ResourceDTO aResource, BoeTaskElementDTO task, HashSet<PerformingOrgDTO> perfOrgsFromDb, IDictionary<int, SpreadCurveModelView> allSpreadCurves, BOEExportInputs exportInputs)
        {
            PerformingOrgDTO perfOrg = resourceType.PerformingOrgID.HasValue ? perfOrgsFromDb.First(x => x.Id == resourceType.PerformingOrgID.Value) : new PerformingOrgDTO();
            string percentSpread = this.sEmpty;
            if (resourceType.SpreadType == SpreadType.Hours)
            {
                if (task.TaskElementType == TaskElementType.Labor && resourceType.PercentSpread.HasValue)
                {
                    // Divide by 100 because the Excel Percentage category will multiply it by 100
                    percentSpread = (resourceType.PercentSpread.Value/100).ToString();
                }
                else
                {
                    percentSpread = CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + this.sEmpty;
                }
            }

            return new string[]
                        {
                                    this.sEmpty, // Total Task Hours
                                    this.sEmpty, // Total Task Cost
                                    resourceType.ResourceID.HasValue ? aResource.ElementOfCost.ToString() : this.sEmpty,
                                    resourceType.ResourceID.HasValue ? aResource.ResourceDesc : this.sEmpty,
                                    resourceType.ResourceID.HasValue ? aResource.LaborType : this.sEmpty,
                                    resourceType.ResourceID.HasValue ? aResource.ResourceName : this.sEmpty,
                                    resourceType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgName : this.sEmpty,
                                    resourceType.PerformingOrgID.HasValue ? perfOrg.PerformingOrgDesc : this.sEmpty,
                                    resourceType.SpreadCurveID.HasValue ? allSpreadCurves[(int)resourceType.SpreadCurveID.Value].SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(exportInputs.Workspace)) : this.sEmpty,
                                    percentSpread
                        };
        }

        /// <summary>
        /// Gets the Resource Type Hours and Cost Spreads
        /// </summary>
        /// <param name="resourceType">Resource Type</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// strings for the Resource Type Hours and Cost Spreads
        /// </returns>
        private string[] GetResourceTypeHourAndCostSpreads(ResourceTypeDto resourceType, BOEExportInputs exportInputs)
        {
            return new string[]
                {
                            (resourceType.SpreadType == SpreadType.Hours && resourceType.ValueSpread.HasValue)
                                ? CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + resourceType.ValueSpread.Value.ToString(Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision))
                                : this.sEmpty,
                            (resourceType.SpreadType == SpreadType.Cost && resourceType.ValueSpread.HasValue)
                                ? CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + resourceType.ValueSpread.Value.ToString(Utilities.CostPrecisionFormattingString(exportInputs.Workspace.CostDecimalPrecision))
                                : this.sEmpty
                };
        }

        /// <summary>
        /// Gets the Resource-Level Custom Fields
        /// </summary>
        /// <param name="row"></param>
        /// <param name="resourceType"></param>
        /// <param name="workspace_customFields"></param>
        /// <param name="workspaceCustomFieldValues"></param>
        private void GetResourceCustomFields(List<string> row, ResourceTypeDto resourceType, IReadOnlyCollection<CustomFieldDTO> workspace_customFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues)
        {
            foreach (CustomFieldDTO resourceCustomField in workspace_customFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
            {
                ICollection<CustomFieldValueDTO> customFieldValues = workspaceCustomFieldValues.Where(i => i.CustomFieldID == resourceCustomField.Id).ToCollection();
                CustomFieldValueDTO customFieldValueForResource = customFieldValues.FirstOrDefault(c => resourceType.CustomFieldValueContainers.Select(r => r.CustomFieldValueID).Contains(c.CustomFieldValueID));
                row.Add(customFieldValueForResource != null ? customFieldValueForResource.CustomFieldValueDescription : this.sEmpty);
            }
        }

        /// <summary>
        /// Gets the ODC Resources
        /// </summary>
        /// <param name="odc">ODC Task</param>
        /// <returns>ODC Resources</returns>
        private HashSet<ResourceDTO> GetOdcResourcesFromDb(OtherDirectCostDTO odc)
        {
            return new HashSet<ResourceDTO>(this.resourceDTODataLoader.GetByIds(odc.ODCTypes.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
        }

        /// <summary>
        /// Gets the ODC Perf Orgs
        /// </summary>
        /// <param name="odc">ODC Task</param>
        /// <returns>the ODC Perf Orgs</returns>
        private HashSet<PerformingOrgDTO> GetOdcPerfOrgsFromDb(OtherDirectCostDTO odc)
        {
            return new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(odc.ODCTypes.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));
        }

        /// <summary>
        /// Gets the WBS sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        private ExcelExportWorksheet GetWBSSheetExportData(BOEExportInputs exportInputs)
        {
            // Get Workspace WBSs
            IReadOnlyCollection<FullWbs> wbs = exportInputs.WbsElementsNoMultiWbs;

            // Get WBSID to CLIN String mappings
            Dictionary<int, string> clinStrings = (from w in wbs
                               select new
                               {
                                   WBSID = w.Id,
                                   ClinString = string.Join(", ", w.Clins.Select(c => c.ClinNumber).OrderBy(c => c))
                               }).ToDictionary(w => w.WBSID, w => w.ClinString);

            // Reuse WBS Exporter since formats are the same
            ExcelExportWorksheet toReturn = this.wbsExporter.GetExcelExportWorksheet(wbs.ToCollection<WbsDTO>(), clinStrings);
            toReturn.WorksheetName = "WBS";

            return toReturn;
        }

        /// <summary>
        /// Gets the cli ns sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        private ExcelExportWorksheet GetCLINsSheetExportData(BOEExportInputs exportInputs, ICollection<PickListDto> contractTypes)
        {
            // Get Workspace CLINs
            Collection<FullClin> clins = exportInputs.ClinsNoMultiClin.ToCollection();

            // Reuse CLIN Exporter since formats are the same
            ExcelExportWorksheet toReturn = this.clinExporter.GetExcelExportWorksheet(clins, contractTypes);
            toReturn.WorksheetName = "CLINs";

            return toReturn;
        }

        /// <summary>
        /// Gets the user permissions sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        private ExcelExportWorksheet GetUserPermissionsSheetExportData(BOEExportInputs exportInputs)
        {
            //TODO: Add code to look up the group accounts from active directory and then add them to the export.
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet("User Permissions");

            // Get workspace admin and workspace BOE potential permissions
            Collection<PermissionsDTO> workspacePotentialPermissions = this.permissionsDTOLoader.GetBOEPotentialPermissionsForWorkspace(exportInputs.Workspace.Id);
            Collection<PermissionsDTO> workspacePermissions = this.permissionsDTOLoader.GetWorkspacePermissions(exportInputs.Workspace.Id);

            // Combine Admin and BOE permissions and group by userID, groupID and displayName, then get all roles for each grouping
            var combinedPermissions = from p in workspacePotentialPermissions.Union(workspacePermissions)
                                      group p by new
                                      {
                                          p.ETIUserId,
                                          this.UserDTODataLoader.GetUserByID(p.ETIUserId).DisplayName
                                      }
                                          into permissionsGroup
                                          orderby permissionsGroup.Key.DisplayName
                                          select new
                                          {
                                              permissionsGroup.Key.ETIUserId,
                                              permissionsGroup.Key.DisplayName,
                                              Roles = permissionsGroup.Select(p => p.Role)
                                          };

            // Output to sheet
            foreach (var workspacePotentialPermission in combinedPermissions)
            {
                UserDTO user = this.UserDTODataLoader.GetUserByID(workspacePotentialPermission.ETIUserId);
                bool isADGroup = user.NTID.Contains('.');
                if (isADGroup)
                {
                    toReturn.Add(
                        workspacePotentialPermission.ETIUserId.ToString(),
                        user.DisplayName,
                        string.Empty,
                        workspacePotentialPermission.Roles.Contains(Role.WorkspaceAdmin) ? this.sYes : this.sNo,
                        workspacePotentialPermission.Roles.Contains(Role.Author) ? this.sYes : this.sNo,
                        workspacePotentialPermission.Roles.Contains(Role.SubcontractorAuthor) ? this.sYes : this.sNo,
                        workspacePotentialPermission.Roles.Contains(Role.WorkspaceReviewer) ? this.sYes : this.sNo,
                        workspacePotentialPermission.Roles.Contains(Role.Approver) ? this.sYes : this.sNo,
                        workspacePotentialPermission.Roles.Contains(Role.SubcontractAdmin) ? this.sYes : this.sNo);
                }
                else
                {
                    toReturn.Add(
                     workspacePotentialPermission.ETIUserId.ToString(),
                     string.Empty,
                     user.DisplayName,
                     workspacePotentialPermission.Roles.Contains(Role.WorkspaceAdmin) ? this.sYes : this.sNo,
                     workspacePotentialPermission.Roles.Contains(Role.Author) ? this.sYes : this.sNo,
                     workspacePotentialPermission.Roles.Contains(Role.SubcontractorAuthor) ? this.sYes : this.sNo,
                     workspacePotentialPermission.Roles.Contains(Role.WorkspaceReviewer) ? this.sYes : this.sNo,
                     workspacePotentialPermission.Roles.Contains(Role.Approver) ? this.sYes : this.sNo,
                     workspacePotentialPermission.Roles.Contains(Role.SubcontractAdmin) ? this.sYes : this.sNo);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the workspace identification sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>

        protected abstract ExcelExportWorksheet GetWorkspaceIdentificationSheetExportData(BOEExportInputs exportInputs);

        /// <summary>
        /// Gets the export data for the Travel Unit Cost sheet
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>Export data</returns>
        protected virtual ExcelExportWorksheet GetTravelUnitCostSheetExportData(FullWorkspace workspace)
        {
            // return null for ssc/default
            return null;
        }

        /// <summary>
        /// Gets the export data for the Travel Extended Cost sheet
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>Export data</returns>
        protected virtual ExcelExportWorksheet GetTravelExtendedCostSheetExportData(FullWorkspace workspace)
        {
            // return null for ssc/default
            return null;
        }
    }
}
