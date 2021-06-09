// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using IES.Common.classes;

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Project Map Workspace Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProjectMapExporter
    {
        /// <summary>
        /// Exports Project Map Workspace to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Project Map Workspace Excel file template</param>
        /// <param name="projectMapData">The Project Map data.</param>
        /// <param name="workspace">The full workspace.</param>
        /// <param name="isOffloaded">if set to <c>true</c> [is offloaded].</param>
        /// <returns>Path to the exported Project Map file</returns>
        public static string ExportToExcelFile(string templateFileLocation, IReadOnlyCollection<ProjectMapModelView> projectMapData, FullWorkspace workspace, bool isOffloaded)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (projectMapData == null)
            {
                throw new ArgumentNullException(nameof(projectMapData));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Get all of the legacy resources.
            ICommonDataMapper commonDataMapper = GenBOEUnityContainer.Resolve<ICommonDataMapper>();
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = commonDataMapper.GetSikorskyLegacyResourcesDictionary(workspace.IsProjectMapWorkspace);

            // Create a new random file name in the specified directory
            string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            // Create the document object in memory
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
            {
                PopulateOptionsList(spreadsheet, workspace);
                PopulateResources(projectMapData, spreadsheet, workspace.ProjectMapType, isOffloaded, commonDataMapper, allLegacyResources);
            }

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Populates the ProjectMap resources.
        /// </summary>
        /// <param name="projectMapData">The Project Map data.</param>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="projectMapType">Type of the project map.</param>
        /// <param name="isOffloaded">if set to <c>true</c> [is offloaded].</param>
        /// <param name="commonDataMapper">The CommonDataMapper instance.</param>
        /// <param name="allLegacyResources">All of the Sikorsky Legacy Resources.</param>
        private static void PopulateResources(IReadOnlyCollection<ProjectMapModelView> projectMapData, SpreadsheetDocument spreadsheet, ProjectMapType projectMapType, bool isOffloaded, ICommonDataMapper commonDataMapper, IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources)
        {
            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();
            bool isProjectMap = projectMapType == ProjectMapType.NonTimePhasedProjectMap || projectMapType == ProjectMapType.TimePhasedProjectMap;
            bool nonTimePhased = projectMapType != ProjectMapType.TimePhasedProjectMap;

            Collection<string> headers = new Collection<string>()
            {
                ImportExportConstants.PROJECTMAP_WBS_NUMBER_COLUMN_HEADER,
                ImportExportConstants.ACTIVITY_ID_COLUMN_HEADER,
                ImportExportConstants.ACTIVITY_NAME_COLUMN_HEADER,
                ImportExportConstants.WBS_ELEMENT_TITLE_COLUMN_HEADER,
                ImportExportConstants.INITIAL_RESOURCE_COLUMN_HEADER,
                ImportExportConstants.COST_CENTER_COLUMN_HEADER,
                ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER,
                ImportExportConstants.START_DATE_COLUMN_HEADER,
                ImportExportConstants.END_DATE_COLUMN_HEADER,
                ImportExportConstants.CLIN_COLUMN_HEADER,
                ImportExportConstants.SOW_COLUMN_HEADER,
                ImportExportConstants.SOW_TITLE_COLUMN_HEADER,
                ImportExportConstants.TASK_COLUMN_HEADER,
                ImportExportConstants.HOURS_COLUMN_HEADER,
                ImportExportConstants.DOLLARS_COLUMN_HEADER,
                (nonTimePhased) ? ImportExportConstants.RATIONALE_COLUMN_HEADER : ImportExportConstants.BOE_COLUMN_HEADER,
                ImportExportConstants.CAM_NAME_COLUMN_HEADER,
                ImportExportConstants.CATEGORY_COLUMN_HEADER
            };

            if (!isOffloaded) { headers.Add(ImportExportConstants.OFFLOAD_COLUMN_HEADER); }

            headers.Add(ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER);
            headers.Add(ImportExportConstants.ADD_DELETE_COLUMN_HEADER);

            if(isProjectMap) { headers.Add(ImportExportConstants.TIERED_PERCENTAGE_COLUMN_HEADER); }

            if (!nonTimePhased)
            {
                // Add 17 years of months for bucketized/time-phased project maps
                for (int i = 1; i <= 204; i++)
                {
                    headers.Add("M" + i.ToString());
                }
            }

            worksheet.Add(headers);

            if (projectMapData.Any())
            {
                foreach (ProjectMapModelView data in projectMapData)
                {
                    Collection<string> row = new Collection<string>
                    {
                        CommonConstants.FORCE_AS_STRING_VALUE + data.WbsNumber,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.ActivityID,
                        CommonConstants.FORCE_AS_STRING_VALUE + RTEUtilities.TurnHTMLIntoPlainText(data.ActivityName),
                        CommonConstants.FORCE_AS_STRING_VALUE + data.WbsElementTitle,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.InitialResource,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.CostCenter,
                        commonDataMapper.GetSikorskyLegacyResourceID(data.LegacyID, allLegacyResources),
                        data.StartDate?.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR),
                        data.EndDate?.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR),
                        CommonConstants.FORCE_AS_STRING_VALUE + data.Clin,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.SowNumber,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.SowTitle,
                        CommonConstants.FORCE_AS_STRING_VALUE + RTEUtilities.TurnHTMLIntoPlainText(data.Task),
                        data.Hours.ToString(),
                        data.Dollars.ToString(),
                        CommonConstants.FORCE_AS_STRING_VALUE + RTEUtilities.TurnHTMLIntoPlainText(data.Rationale),
                        CommonConstants.FORCE_AS_STRING_VALUE + data.CamName,
                        CommonConstants.FORCE_AS_STRING_VALUE + data.Category
                    };

                    if (!isOffloaded) { row.Add(data.Offload.ToString().ToUpper()); }

                    row.Add(CommonConstants.FORCE_AS_STRING_VALUE + data.ClassOfCost);
                    row.Add(data.AddDelete ?? "A");

                    if (isProjectMap) { row.Add(data.TieredPercentage.ToString()); }

                    if (!nonTimePhased) { AddDiscreteMonths(data, row); }

                    worksheet.Add(row);
                }
            }

            // Export the data to the worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, "PROJECTMAP");
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 1);

            AddDataValidation(worksheetPart, projectMapData.Count, isOffloaded);

            string sheetRange = ExcelUtilities.RedefineSheetDimensions(worksheetPart, ((uint)projectMapData.Count) + 1U, 0);
            ExcelUtilities.SetIgnoredErrors(worksheetPart.Worksheet, sheetRange);

            // save the worksheet
            worksheetPart.Worksheet.Save();
        }

        /// <summary>
        /// Adds the discrete months to the end of the row.
        /// </summary>
        /// <param name="data">The project map data.</param>
        /// <param name="row">The row (of strings).</param>
        private static void AddDiscreteMonths(ProjectMapModelView data, Collection<string> row)
        {
            foreach (decimal? discreteMonth in data.DiscreteMonths)
            {
                row.Add(discreteMonth?.ToString() ?? string.Empty);
            }
        }

        /// <summary>
        /// Populates the options list worksheet.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="workspace">The workspace.</param>
        private static void PopulateOptionsList(SpreadsheetDocument spreadsheet, FullWorkspace workspace)
        {
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

            IReadOnlyCollection<ResourceDTO> resources = workspace.ResourcesForWsResourceListId.Where(r => r.ElementOfCost == ElementOfCostType.LMLabor || r.ElementOfCost == ElementOfCostType.IWTA || r.ElementOfCost == ElementOfCostType.Travel || r.ElementOfCost == ElementOfCostType.ODC).OrderBy(x => x.ResourceName).ToList();
            IReadOnlyCollection<PerformingOrgDTO> perfOrgs = workspace.PerformingOrgsForWsList.OrderBy(x => x.PerformingOrgName).ToList();
            ICommonDataMapper commonDataMapper = GenBOEUnityContainer.Resolve<ICommonDataMapper>();
            IReadOnlyCollection<SikorskyLegacyResourceDTO> legacyResources = commonDataMapper.GetSikorskyLegacyResources();
            string[] offloadOptions = new string[] { "TRUE", "FALSE" };
            string[] addDeleteOptions = new string[] { "A", "D" };
            string[] classOfCostOptions = new string[] { ClassOfCost.Recurring.GetDescription(), ClassOfCost.NonRecurring.GetDescription() };

            // Add DevNonRecurring Class of Cost option if workspace is using it
            CustomFieldDTO classOfCostCf = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName == Constants.SikorskyConstants.SIKORSKY_CF_CLASSOFCOST);
            if (classOfCostCf != null)
            {
                ICollection<CustomFieldValueDTO> cfOptions = workspace.CustomFieldValues.Where(x => x.CustomFieldID == classOfCostCf.Id).ToCollection();
                if (cfOptions != null && cfOptions.Any(x => x.CustomFieldValueName == ClassOfCost.DevNonRecurring.GetDescription()))
                {
                    classOfCostOptions = new string[] { ClassOfCost.Recurring.GetDescription(), ClassOfCost.NonRecurring.GetDescription(), ClassOfCost.DevNonRecurring.GetDescription() };
                }
            }

            // Options List Column Headers
            List<string> headerValues = new List<string>
            {
                ImportExportConstants.RESOURCE_COLUMN_HEADER,
                ImportExportConstants.PERF_ORG_COLUMN_HEADER,
                ImportExportConstants.OFFLOAD_COLUMN_HEADER,
                ImportExportConstants.ADD_DELETE_COLUMN_HEADER,
                ImportExportConstants.CLASS_OF_COST_COLUMN_HEADER,
                ImportExportConstants.LEGACY_RESOURCE_COLUMN_HEADER
            };

            int maxRows = Math.Max(addDeleteOptions.Length, Math.Max(offloadOptions.Length, Math.Max(resources.Count, perfOrgs.Count)));

            optionsListWorksheet.Add(headerValues);   // Add option list header row

            // Add option value rows
            for (int i = 0; i < maxRows; i++)
            {
                List<string> optionValues = new List<string>
                {
                    resources.Count > i ? resources.ElementAt(i).ResourceName : string.Empty,
                    perfOrgs.Count > i ? perfOrgs.ElementAt(i).PerformingOrgName : string.Empty,
                    offloadOptions.Length > i ? offloadOptions[i] : string.Empty,
                    addDeleteOptions.Length > i ? addDeleteOptions[i] : string.Empty,
                    classOfCostOptions.Length > i ? classOfCostOptions[i] : string.Empty,
                    legacyResources.Count > i ? legacyResources.ElementAt(i).LegacyResourceID : string.Empty
                };

                optionsListWorksheet.Add(optionValues);
            }

            // Export the Options List headers and data to row 1 of the "Options List" worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsListWorksheet.WorksheetName);
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsListWorksheet, 1);

            // Adjust existing Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { ImportExportConstants.RESOURCES, resources.Count },
                { ImportExportConstants.PERFORGS, perfOrgs.Count },
                { ImportExportConstants.OFFLOAD, offloadOptions.Length },
                { ImportExportConstants.ADD_DELETE, addDeleteOptions.Length },
                { ImportExportConstants.CLASS_OF_COST, classOfCostOptions.Length },
                { ImportExportConstants.LEGACY_RESOURCES, legacyResources.Count }
            };
            ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);
        }

        /// <summary>
        /// Adds the data validation dropdowns to the excel spreadsheet.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        /// <param name="projectMapResourcesCount">The project map resources count.</param>
        /// <param name="isOffloaded">if set to <c>true</c> [is offloaded].</param>
        private static void AddDataValidation(WorksheetPart worksheetPart, int projectMapResourcesCount, bool isOffloaded)
        {
            // Create the data validation dropdowns for custom fields
            uint startDataRowIndex = 2;
            int startDataColIndex = 1;
            uint endDataRowIndex = (uint)(projectMapResourcesCount + 11); // all of the data rows and ten extra

            // Adjust the spread offset by the custom fields and multi columns 
            Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();

            // Add data validation references for Resource, Performing Org, and Spread Curve columns
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCES,
                ImportExportConstants.PROJECTMAP_RESOURCE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.PERFORGS,
                ImportExportConstants.PROJECTMAP_PERFORG_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.LEGACY_RESOURCES,
                ImportExportConstants.PROJECTMAP_LEGACY_RESOURCE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

            // offset in case the offload column is there or not
            int offloadColumnOffset = -1;
            if (!isOffloaded)
            {
                ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.OFFLOAD,
                    ImportExportConstants.PROJECTMAP_OFFLOAD_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

                offloadColumnOffset = 0;
            }

            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CLASS_OF_COST,
                    ImportExportConstants.PROJECTMAP_CLASS_OF_COST_CELL_COLUMN_OFFSET + offloadColumnOffset + startDataColIndex, startDataRowIndex, endDataRowIndex);

            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.ADD_DELETE,
                ImportExportConstants.PROJECTMAP_ADD_DELETE_CELL_COLUMN_OFFSET + offloadColumnOffset + startDataColIndex, startDataRowIndex, endDataRowIndex);

            ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
        }
    }
}