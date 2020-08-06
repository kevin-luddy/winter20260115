// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Drawing.Spreadsheet;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;
    using A = DocumentFormat.OpenXml.Drawing;
    using Shape = DocumentFormat.OpenXml.Drawing.Spreadsheet.Shape;
    using Table = DocumentFormat.OpenXml.Spreadsheet.Table;


    /// <summary>
    /// Used for exporting one or more BOEs based on a pre-formatted Workoffline template
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WorkofflineExporter
    {
        ICommonDataMapper _ICommonDataMapper;
        IResourceDTODataLoader _IResourceDTODataLoader;
        IPerformingOrgDTODataLoader perfOrgLoader;
        ICustomFieldValueDTODataLoader _ICustomFieldValueDTODataLoader;

        public WorkofflineExporter(
            ICommonDataMapper inICommonDataMapper,
            IResourceDTODataLoader inIResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            ICustomFieldValueDTODataLoader inICustomFieldValueDTODataLoader)
        {
            this._ICommonDataMapper = inICommonDataMapper;
            this._IResourceDTODataLoader = inIResourceDTODataLoader;
            this.perfOrgLoader = perfOrgLoader;
            this._ICustomFieldValueDTODataLoader = inICustomFieldValueDTODataLoader;
        }

        /// <summary>
        /// Generate the Workoffline spreadsheet for the specified workspace and BOE IDs, and return as a byte array.
        /// </summary>
        /// <param name="templateFileLocation">Full path to the Excel template file</param>
        /// <param name="workspace">Workspace</param>
        /// <param name="selectedBoes">BOEs to be exported</param>
        /// <returns>Excel spreadsheet contents</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
        private byte[] ExportToExcelStream(
            string templateFileLocation,
            FullWorkspace workspace,
            ICollection<FullBoe> selectedBoes)
        {
            // Check inputs
            if (templateFileLocation == null) { throw new ArgumentNullException(nameof(templateFileLocation)); }
            if (workspace == null) { throw new ArgumentNullException(nameof(workspace)); }
            if (selectedBoes == null) { throw new ArgumentNullException(nameof(selectedBoes)); }

            byte[] documentStream = null;

            // open a copy of the Excel template file into memory
            byte[] byteArray = File.ReadAllBytes(templateFileLocation);

            using (MemoryStream memory = new MemoryStream())
            {
                memory.Write(byteArray, 0, byteArray.Length);

                // synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
                lock (CacheConstants.OPEN_XML_LOCK)
                {
                    // Create the document object in memory
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(memory, true))
                    {
                        // refresh in use flag once
                        this._ICustomFieldValueDTODataLoader.RefreshCustomFieldInUseByWorkspaceID(workspace.Id);

                        this.PopulateOptionsList(spreadsheet, workspace);

                        // process the spreadsheet document
                        this.ExportBoeDataDelegate(spreadsheet, workspace, selectedBoes);
                    }

                    // Pull the resulting document from memory into the byte array
                    documentStream = memory.ToArray();
                }
            }

            return documentStream;
        }

        /// <summary>
        /// Generate the Workoffline spreadsheet for the specified workspace and BOE IDs, and apply output directly to the HTTP response stream.
        /// </summary>
        /// <param name="templateFileLocation">Full path to the Excel template file</param>
        /// <param name="workspace">Workspace</param>
        /// <param name="selectedBoes">BOEs to be exported</param>
        /// <param name="Response">the web response object to write the file back to for user download</param>
        /// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
        public void ExportToExcelResponse(
            string templateFileLocation,
            FullWorkspace workspace,
            ICollection<FullBoe> selectedBoes,
            HttpResponseBase Response,
            string fileNameToDisplayToBrowser)
        {
            if (Response == null)
            {
                throw new ArgumentNullException(nameof(Response));
            }

            byte[] documentStream = this.ExportToExcelStream(templateFileLocation, workspace, selectedBoes);

            // setup the response correctly with BufferOutput since this is going to be awhile...
            Response.ContentType = BOEExporterConstants.ContentType_XLSM;
            Response.Clear();
            Response.BufferOutput = true;
            Response.AppendHeader(BOEExporterConstants.CONTENT_HEADER_NAME, string.Format(BOEExporterConstants.CONTENT_HEADER_FORMAT_STRING, fileNameToDisplayToBrowser));

            Response.OutputStream.Write(documentStream, 0, documentStream.Length);
        }

        /// <summary>
        /// Export a worksheet for each of the specified BOEs.  Hide all other worksheets.
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workspace"></param>
        /// <param name="boes"></param>
        private void ExportBoeDataDelegate(SpreadsheetDocument spreadsheet, FullWorkspace workspace, ICollection<FullBoe> boes)
        {
            //-----------------------------------------------------------------------------------------------------------------
            // The hidden "Table Templates" worksheet contains the baseline layout and formatting information for the
            // workspace, BOE, Task, and Resource tables.  There is also a special table called "DynamicColumns" with 
            // formatting information for dynamically generated Custom Field columns, and monthly Hours & Cost columns.
            // In order for the "Add New Task" and "Add Resource" buttons to function properly, we need to update
            // the template tables by adding any custom fields defined for the current workspace.
            //-----------------------------------------------------------------------------------------------------------------

            // Get baseline template information
            WorkofflineTemplate workofflineTemplate = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);

            // Get a list of all the clins and WBS
            Collection<FullWbs> wsWBSs = new Collection<FullWbs>(workspace.WbsElementsNoMultiWbs.ToCollection());
            Collection<ClinDTO> wsClins = new Collection<ClinDTO>(workspace.ClinsNoMultiClin.ToCollection<ClinDTO>());

            // Remove existing template tables and data from the template worksheet
            ExcelUtilities.RemoveTablesAndData(workofflineTemplate.TemplateSheetPart);

            // Regenerate the TemplateTables worksheet with space allocated for custom fields.
            // Do this by calling the ExportBOE method for the TemplateTables worksheet with null BOE, WBS and CLIN objects.
            // This will generate empty Workspace, BOE, Task and Resource tables with custom fields.
            this.ExportBOE(spreadsheet, workofflineTemplate, workofflineTemplate.TemplateSheetPart,
                ImportExportConstants.TABLE_TEMPLATES, null, null, null, null, workspace.CustomFields, wsWBSs, wsClins);

            // Re-Initialize template information based on updated tables in the Table Templates worksheet.
            workofflineTemplate = new WorkofflineTemplate(spreadsheet, ImportExportConstants.TABLE_TEMPLATES);

            //-----------------------------------------------------------------------------------------------------------------
            // At this point, we are ready to generate individual worksheets for each of the selected BOEs.
            // For each BOE ID, create a new worksheet and populate Workspace, BOE, Task and Resource tables.
            //-----------------------------------------------------------------------------------------------------------------
            foreach (FullBoe boe in boes)
            {
                boe.LoadTaskElementRTEData();

                WbsDTO wbs = boe.Wbs;
                ClinDTO clin = boe.Clin;

                // Generate a unique worksheet name.
                string worksheetName = "BOE " + boe.Id;

                // Export the BOE to the worksheet
                WorksheetPart newWorksheetPart = ExcelUtilities.CopyWorksheet(spreadsheet,
                    ImportExportConstants.TABLE_TEMPLATES, worksheetName, false);
                this.ExportBOE(spreadsheet, workofflineTemplate, newWorksheetPart,
                    worksheetName, workspace, boe, wbs, clin, workspace.CustomFields, wsWBSs, wsClins);
            }

            //-----------------------------------------------------------------------------------------------------------------
            // Finally, perform any miscellaneous clean-up work to make the spreadsheet presentable for the user.
            //-----------------------------------------------------------------------------------------------------------------
            ExcelUtilities.HideWorksheets(spreadsheet, ImportExportConstants.ExcludedSheetNames);
            // Deselect all worksheet tabs.  So when the document is opened, the first visible worksheet will be selected.
            ExcelUtilities.DeselectAllWorksheetTabs(spreadsheet);
        }

        /// <summary>
        /// Export all data for a BOE to the specified worksheet.
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="worksheetName"></param>
        /// <param name="workspace"></param>
        /// <param name="boe"></param>
        /// <param name="wbs"></param>
        /// <param name="clin"></param>
        private void ExportBOE(SpreadsheetDocument spreadsheet, WorkofflineTemplate workofflineTemplate, WorksheetPart worksheetPart, string worksheetName,
            FullWorkspace workspace, FullBoe boe, WbsDTO wbs, ClinDTO clin, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, IReadOnlyCollection<FullWbs> wsWBSs, IReadOnlyCollection<ClinDTO> wsClins)
        {
            // Get the ID of the "Add Resource" button
            uint addResourceButtonId = FindButtonByName(worksheetPart, ImportExportConstants.ADD_RESOURCE_BUTTON);

            uint tableDefPartId = ExcelUtilities.GetMaxTableDefPartId(spreadsheet);

            // Maintain a collection of TablePartDefinitions and associated data (Workspace, BOE, Tasks and Resources)
            Collection<string> tablePartDefinitionIds = new Collection<string>();

            // Maintain a collection of Data Validation references
            Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();   // map defined name to data validation cell references

            // Generate Workspace table
            uint rowIndex = 1;
            if (workofflineTemplate.WorkspaceTable != null)
            {
                string workspaceTableName = (boe == null) ?
                    ImportExportConstants.WORKSPACE_TABLE_TEMPLATE :                                // Updating Workspace table template or
                    worksheetName.Replace(" ", "") + ImportExportConstants.WORKSPACE_TABLE_SUFFIX;  // Adding Workspace table to BOE worksheet
                TableDefinitionPart workspaceTableDefinitionPart = this.GenerateWorkspaceWorksheetData(spreadsheet,
                    workofflineTemplate, worksheetPart, ++tableDefPartId, workspaceTableName, rowIndex, workspace);
                tablePartDefinitionIds.Add(worksheetPart.GetIdOfPart(workspaceTableDefinitionPart));
                TableRange tableRange = new TableRange(worksheetName, workspaceTableDefinitionPart.Table);
                rowIndex = tableRange.RowEnd + 3;
            }

            // Generate Boe table
            if (workofflineTemplate.BoeTable != null)
            {
                string boeTableName = (boe == null) ?
                     ImportExportConstants.BOE_TABLE_TEMPLATE :                                 // Updating BOE table template or
                     worksheetName.Replace(" ", "") + ImportExportConstants.BOE_TABLE_SUFFIX;   // Adding BOE table to BOE worksheet
                TableDefinitionPart boeTableDefinitionPart = this.GenerateBoeWorksheetData(spreadsheet,
                    workofflineTemplate, worksheetPart, ++tableDefPartId, boeTableName, rowIndex, boe, wbs, clin,
                    allWorkspaceCustomFields, dataValidationReferences, workspace?.RteOverrides.ToList());
                tablePartDefinitionIds.Add(worksheetPart.GetIdOfPart(boeTableDefinitionPart));
                TableRange tableRange = new TableRange(worksheetName, boeTableDefinitionPart.Table);
                rowIndex = tableRange.RowEnd + 3;
            }

            // Export task and resource table definitions
            if (boe == null)
            {
                // Export empty tables with space allocated for custom resource fields
                rowIndex = this.GenerateTaskAndResourceTables(
                    spreadsheet, 
                    workofflineTemplate, 
                    worksheetPart, 
                    worksheetName, 
                    workspace,
                    addResourceButtonId, 
                    tablePartDefinitionIds, 
                    allWorkspaceCustomFields, 
                    dataValidationReferences,
                    rowIndex, 
                    wsClins,
                    wsWBSs,
                    null, 
                    null,
                    ImportExportConstants.TASK_TABLE_TEMPLATE, 
                    ImportExportConstants.RESOURCE_TABLE_TEMPLATE,
                    ++tableDefPartId, 
                    ++tableDefPartId, 
                    true);
            }
            else
            {
                // For each Task, export task and resource table definitions
                ICollection<BoeTaskElementDTO> taskElements = boe.TaskElements.ToList();
                if (taskElements.Any())
                {
                    int taskIndex = 0;  // used to uniquely name the task and resource tables
                    foreach (BoeTaskElementDTO taskElement in taskElements)
                    {
                        taskIndex++;
                        string newTaskTableName = worksheetName.Replace(" ", "") + ImportExportConstants.TASK_TABLE_SUFFIX + taskIndex.ToString("D6");
                        string newResourceTableName = worksheetName.Replace(" ", "") + ImportExportConstants.RESOURCE_TABLE_SUFFIX + taskIndex.ToString("D6");

                        rowIndex = this.GenerateTaskAndResourceTables(spreadsheet, workofflineTemplate, worksheetPart, worksheetName, workspace,
                            addResourceButtonId, tablePartDefinitionIds, allWorkspaceCustomFields, dataValidationReferences,
                            rowIndex,wsClins,wsWBSs, taskElement, boe, newTaskTableName, newResourceTableName, ++tableDefPartId, ++tableDefPartId, (taskIndex == 1));
                    }
                }
                else
                {
                    // No task elements for this BOE.
                    // Remove the cloned "Add Resource" Button (no resource tables, so button is not needed)
                    RemoveButton(worksheetPart, addResourceButtonId);
                }
            }

            // if we're regenerating the template tables
            if (boe == null)
            {
                // Regenerate the Dynamic Columns Template
                rowIndex = this.RegenerateDynamicColumnsTemplate(workofflineTemplate, worksheetPart, tablePartDefinitionIds, ++tableDefPartId, rowIndex);
            }

            // Add data validation references to worksheet definition            
            if (dataValidationReferences.Count > 0)
            {
                DataValidations oldDataValidations = worksheetPart.Worksheet.Elements<DataValidations>().FirstOrDefault();
                DataValidations newDataValidations = GenerateDataValidations(dataValidationReferences);
                if (oldDataValidations == null)
                {
                    worksheetPart.Worksheet.InsertAfter<DataValidations>(newDataValidations, worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault<SheetData>());
                }
                else
                {
                    worksheetPart.Worksheet.ReplaceChild<DataValidations>(newDataValidations, oldDataValidations);
                }
            }

            // Add TableParts to worksheet definition -- they always come last
            TableParts newTableParts = new TableParts() { Count = (UInt32Value)0 };
            foreach (string tablePartId in tablePartDefinitionIds)
            {
                newTableParts.Count++;
                TablePart tablePart = new TablePart() { Id = tablePartId };
                newTableParts.Append(tablePart);
            }
            worksheetPart.Worksheet.RemoveAllChildren<TableParts>();
            worksheetPart.Worksheet.AppendChild<TableParts>(newTableParts);
        }

        /// <summary>
        /// Export the Options List headers and data, including all custom field values
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workspace"></param>
        private void PopulateOptionsList(SpreadsheetDocument spreadsheet, FullWorkspace workspace)
        {
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

            // Options List Column Headers
            List<string> headerValues = new List<string>();
            headerValues.Add(ImportExportConstants.MOQ_TYPE_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.RESOURCE_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.PERF_ORG_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.CLIN_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.WBS_COLUMN_HEADER);

            // Gather options list values
            Collection<MOQTypeModelView> allMOQType = new Collection<MOQTypeModelView>((from x in this._ICommonDataMapper.getMOQType()
                                                                                        select x).ToArray());
            Collection<FullWbs> allWBSs = new Collection<FullWbs>(workspace.WbsElementsNoMultiWbs.ToCollection());
            Collection<ClinDTO> allClins = new Collection<ClinDTO>(workspace.ClinsNoMultiClin.ToCollection<ClinDTO>());

            IReadOnlyCollection<PerformingOrgDTO> allPerformingOrgs = workspace.PerformingOrgsForWsList;
            Collection<SpreadCurveModelView> allCurves = this._ICommonDataMapper.getSpreadCurve();
            ICollection<ResourceDTO> allResourceTypes = this._IResourceDTODataLoader.GetByListIdAndElementOfCost(
                workspace.ResourceListID,
                new Collection<ElementOfCostType>() { ElementOfCostType.LMLabor, ElementOfCostType.IWTA, ElementOfCostType.Sub, ElementOfCostType.Travel, ElementOfCostType.Materials, ElementOfCostType.ODC });

            int maxRows = allMOQType.Count > allPerformingOrgs.Count ? allMOQType.Count : allPerformingOrgs.Count;
            maxRows = maxRows > allCurves.Count ? maxRows : allCurves.Count;
            maxRows = maxRows > allResourceTypes.Count ? maxRows : allResourceTypes.Count;
            maxRows = maxRows > allWBSs.Count ? maxRows : allWBSs.Count;
            maxRows = maxRows > allClins.Count ? maxRows : allClins.Count;

            #region Custom Fields

            // Include custom field values in Options List Data
            IReadOnlyCollection<CustomFieldDTO> customFields = workspace.CustomFields;
            ICollection<CustomFieldValueDTO> customFieldValues = new Collection<CustomFieldValueDTO>();
            if (customFields.Any())
            {
                customFieldValues = this._ICustomFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(customFields.Select(c => c.Id).ToCollection());
            }

            List<List<string>> customFieldValueTable = new List<List<string>>();

            // keep a list of defined names for each custom field 
            Dictionary<string, string> customFieldDefinedNames = new Dictionary<string, string>();

            foreach (CustomFieldDTO customField in customFields.Where(x=>!x.IsOpenEnded))
            {
                headerValues.Add(ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName));    // Options List column header

                // Get all values for the current field
                ICollection<CustomFieldValueDTO> customFieldValuesToCopy = customFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection<CustomFieldValueDTO>();

                List<string> valueList = new List<string>();

                // If there are values to copy, let's do it
                if (customFieldValuesToCopy.Any())
                {
                    // Iterate over each value
                    foreach (CustomFieldValueDTO customFieldValue in customFieldValuesToCopy)
                    {
                        valueList.Add(String.Format("{0} - {1}", customFieldValue.CustomFieldValueName, customFieldValue.CustomFieldValueDescription));
                    }
                }
                customFieldValueTable.Add(valueList);
                maxRows = maxRows > valueList.Count ? maxRows : valueList.Count;
                string colName = ExcelUtilities.GetColumnNameFromColumnIndex(headerValues.Count - 1);
                string definedNameText = (valueList.Count > 1) ? "'" + ImportExportConstants.OPTIONS_LISTS + "'!$" + colName + "$2:$" + colName + "$" + (valueList.Count + 1)
                                                                : "'" + ImportExportConstants.OPTIONS_LISTS + "'!$" + colName + "$2";
                // Create a DefinedName to be used for data validation
                // Defined Names must start with an underscore, or letter and cannot contain 
                // spaces or other special characters.  So, create the DefinedName as "CustomField_" followed by the FieldID.
                customFieldDefinedNames.Add(ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customField.Id, definedNameText);
            }

            #endregion Custom Fields

            optionsListWorksheet.Add(headerValues);   // Add option list header row

            // Add option value rows
            for (int i = 0; i < maxRows; i++)
            {
                List<string> optionValues = new List<string>();
                optionValues.Add(allMOQType.ElementAtOrDefault(i) != null ? allMOQType.ElementAt(i).MOQTypeName : string.Empty);
                optionValues.Add(allResourceTypes.ElementAtOrDefault(i) != null ? allResourceTypes.ElementAt(i).ResourceDesc : string.Empty);
                optionValues.Add(allPerformingOrgs.ElementAtOrDefault(i) != null ? allPerformingOrgs.ElementAt(i).PerformingOrgName + " - " + allPerformingOrgs.ElementAt(i).PerformingOrgDesc : string.Empty);
                optionValues.Add(allCurves.ElementAtOrDefault(i) != null ? allCurves.ElementAt(i).SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(workspace)) : string.Empty);
                optionValues.Add(allClins.ElementAtOrDefault(i) != null ? allClins.ElementAt(i).ClinString : string.Empty);
                optionValues.Add(allWBSs.ElementAtOrDefault(i) != null ? allWBSs.ElementAt(i).WbsString : string.Empty);

                // include custom field values
                foreach (List<string> valueList in customFieldValueTable)
                {
                    optionValues.Add(valueList.Count > i ? valueList[i] : string.Empty);
                }

                optionsListWorksheet.Add(optionValues);
            }

            // Export the Options List headers and data to row 1 of the "Options List" worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsListWorksheet.WorksheetName);
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsListWorksheet, 1);

            // Adjust existing Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { ImportExportConstants.MOQ_TYPES, allMOQType.Count },
                { ImportExportConstants.RESOURCES, allResourceTypes.Count },
                { ImportExportConstants.PERFORGS, allPerformingOrgs.Count },
                { ImportExportConstants.SPREAD_CURVES, allCurves.Count },
                {ImportExportConstants.CLINS, allClins.Count},
                {ImportExportConstants.WBSS, allWBSs.Count}
            };
            ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);

            // Add Defined Names for each custom field
            ExcelExporter.AddDefinedNames(spreadsheet, customFieldDefinedNames);
        }


        /// <summary>
        /// Generate a Workspace table with data
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="newTableDefPartId"></param>
        /// <param name="newTableName"></param>
        /// <param name="rowIndex"></param>
        /// <param name="workspace"></param>
        /// <returns>TableDefinitionPart containing new Workspace table</returns>
        private TableDefinitionPart GenerateWorkspaceWorksheetData(SpreadsheetDocument spreadsheet, WorkofflineTemplate workofflineTemplate,
            WorksheetPart worksheetPart, uint newTableDefPartId, string newTableName, uint rowIndex, WorkspaceDTO workspace)
        {
            uint startRowIndex = rowIndex;
            long startColIndex = 1;
            string workspaceTableBegin = TableRange.GetColumnName(startColIndex) + startRowIndex;
            rowIndex = WorkofflineTemplate.CopyTemplateTable(workofflineTemplate.WorkspaceTable, workofflineTemplate.WorkspaceTableRows, worksheetPart, startColIndex, startRowIndex);
            Cell WorkspaceTableBegin = ExcelUtilities.GetCell(worksheetPart, workspaceTableBegin);

            if (workspace != null)
            {
                // Update cell values as needed
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, WorkspaceTableBegin,
                    ImportExportConstants.WORKSPACE_ID_CELL_ROW_OFFSET, ImportExportConstants.WORKSPACE_ID_CELL_COLUMN_OFFSET,
                    workspace.Id.ToString(), null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, WorkspaceTableBegin,
                    ImportExportConstants.WORKSPACE_NAME_CELL_ROW_OFFSET, ImportExportConstants.WORKSPACE_NAME_CELL_COLUMN_OFFSET,
                    workspace.WorkspaceName, null, true, false);
            }

            // Return the new TableDefinitionPart
            long newColBeginIndex = 1;   // start table in column A (one-based column index)
            long newColEndIndex = newColBeginIndex + (workofflineTemplate.WorkspaceTable.ColumnEndIndex - workofflineTemplate.WorkspaceTable.ColumnBeginIndex);
            TableDefinitionPart workspaceTableDefinitionPart =
                WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(workofflineTemplate.WorkspaceTable,
                    worksheetPart, newTableDefPartId, newTableName, newColBeginIndex, startRowIndex, newColEndIndex, (rowIndex - 1U));
            return workspaceTableDefinitionPart;
        }

        /// <summary>
        /// Generate a BOE table with data
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="newTableDefPartId"></param>
        /// <param name="newTableName"></param>
        /// <param name="rowIndex"></param>
        /// <param name="boe"></param>
        /// <param name="wbs"></param>
        /// <param name="clin"></param>
        /// <param name="allWorkspaceCustomFields"></param>
        /// <param name="dataValidationReferences"></param>
        /// <returns>TableDefinitionPart containing new BOE table</returns>
        private TableDefinitionPart GenerateBoeWorksheetData(SpreadsheetDocument spreadsheet, WorkofflineTemplate workofflineTemplate,
            WorksheetPart worksheetPart, uint newTableDefPartId, string newTableName, uint rowIndex, FullBoe boe, WbsDTO wbs, ClinDTO clin,
            IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, Dictionary<string, string> dataValidationReferences, ICollection<RteTemplateSource> rteOverrides)
        {
            if(rteOverrides == null) { rteOverrides = new List<RteTemplateSource>(); }

            uint startRowIndex = rowIndex;
            long startColIndex = 1; // start table in column A (one-based column index)
            string boeTableBegin = TableRange.GetColumnName(startColIndex) + startRowIndex;
            rowIndex = WorkofflineTemplate.CopyTemplateTable(workofflineTemplate.BoeTable, workofflineTemplate.BoeTableRows, worksheetPart, startColIndex, startRowIndex);
            Cell BoeTableBegin = ExcelUtilities.GetCell(worksheetPart, boeTableBegin);

            if (boe != null)
            {
                // Update cell values as needed
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_ID_CELL_ROW_OFFSET, ImportExportConstants.BOE_ID_CELL_COLUMN_OFFSET,
                    boe.Id.ToString(), null, true, false);
                string wbsInfo = "WBS: " + (wbs == null ? string.Empty : Utilities.FormatNumberTitleString(wbs.WbsNumber, wbs.WbsTitle, " "));
                string clinInfo = "CLIN: " + (clin == null ? string.Empty : Utilities.FormatNumberTitleString(clin.ClinNumber, clin.ClinTitle, " "));
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                   ImportExportConstants.BOE_MULTI_CELL_ROW_OFFSET, ImportExportConstants.BOE_MULTI_CELL_COLUMN_OFFSET,
                   boe.IsMultiClinWbs.ToString(), null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_WBS_CELL_ROW_OFFSET, ImportExportConstants.BOE_WBS_CELL_COLUMN_OFFSET,
                    wbsInfo, null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_CLIN_CELL_ROW_OFFSET, ImportExportConstants.BOE_CLIN_CELL_COLUMN_OFFSET,
                    clinInfo, null, true, false);
                string startDateDisplay = boe.StartDate.ToString("MM/yyyy");
                string endDateDisplay = boe.EndDate.ToString("MM/yyyy");
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_STARTDATE_CELL_ROW_OFFSET, ImportExportConstants.BOE_STARTDATE_CELL_COLUMN_OFFSET,
                    startDateDisplay, null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_ENDDATE_CELL_ROW_OFFSET, ImportExportConstants.BOE_ENDDATE_CELL_COLUMN_OFFSET,
                    endDateDisplay, null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                    ImportExportConstants.BOE_TITLE_CELL_ROW_OFFSET, ImportExportConstants.BOE_TITLE_CELL_COLUMN_OFFSET,
                    boe.Title, null, true, false);

                if(rteOverrides.Contains(RteTemplateSource.BoeDescription))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                        ImportExportConstants.BOE_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.BOE_DESCRIPTION_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT_RTE_TEMPLATES, null, true, false);
                }
                else if (!string.IsNullOrEmpty(boe.Description))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                        ImportExportConstants.BOE_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.BOE_DESCRIPTION_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT, null, true, false);
                }

                if (rteOverrides.Contains(RteTemplateSource.BoeSources))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                        ImportExportConstants.BOE_SOURCESOFDATA_CELL_ROW_OFFSET, ImportExportConstants.BOE_SOURCESOFDATA_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT_RTE_TEMPLATES, null, true, false);
                }
                else if(!string.IsNullOrEmpty(boe.DataSource))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, BoeTableBegin,
                        ImportExportConstants.BOE_SOURCESOFDATA_CELL_ROW_OFFSET, ImportExportConstants.BOE_SOURCESOFDATA_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT, null, true, false);
                }
            
                // Display BOE custom field labels and values
                IDictionary<CustomFieldValueDTO, CustomFieldDTO> boeCustomFields = boe.AssignedBOECustomFieldValues;

                rowIndex = startRowIndex + ImportExportConstants.BOE_FIRST_CUSTOMFIELD_ROW_OFFSET;  // reset rowIndex to start of custom fields
                foreach (CustomFieldDTO customFieldForLabel in allWorkspaceCustomFields)
                {
                    if (customFieldForLabel.CustomFieldDisplayID == CustomFieldType.BoeDisplay)
                    {
                        // set label
                        string customFieldLabel = customFieldForLabel.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customFieldForLabel.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customFieldForLabel.CustomFieldName);

                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, rowIndex, startColIndex + ImportExportConstants.BOE_CUSTOMFIELD_LABEL_CELL_COLUMN_OFFSET,
                            customFieldLabel, workofflineTemplate.BoeCustomFieldNameStyleIndex, true, false);
                        // add data validation for the custom field value cell if not open ended
                        if (!customFieldForLabel.IsOpenEnded)
                        {
                            AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customFieldForLabel.Id,
                                startColIndex + ImportExportConstants.BOE_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET, rowIndex, rowIndex);
                        }

                        // set corresponding value (if there is one)
                        foreach (KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customFieldAndValue in boeCustomFields)
                        {
                            CustomFieldValueDTO fieldValue = customFieldAndValue.Key;
                            CustomFieldDTO field = customFieldAndValue.Value;

                            if (customFieldForLabel.Id == field.Id)
                            {
                                // set value
                                string customFieldText;
                                if(field.IsOpenEnded)
                                {
                                    customFieldText = string.Format("{0}", fieldValue.CustomFieldValueDescription);
                                }
                                else
                                {
                                    customFieldText = string.Format("{0} - {1}", fieldValue.CustomFieldValueName, fieldValue.CustomFieldValueDescription);
                                }
                                ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, rowIndex, startColIndex + ImportExportConstants.BOE_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET,
                                    customFieldText, workofflineTemplate.BoeCustomFieldValueStyleIndex, true, false);
                            }
                        }

                        rowIndex++;
                    }
                }
            }

            // Return the new TableDefinitionPart
            long endColIndex = startColIndex + (workofflineTemplate.BoeTable.ColumnEndIndex - workofflineTemplate.BoeTable.ColumnBeginIndex);
            TableDefinitionPart boeTableDefinitionPart =
                WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(workofflineTemplate.BoeTable,
                    worksheetPart, newTableDefPartId, newTableName, startColIndex, startRowIndex, endColIndex, (rowIndex - 1U));
            return boeTableDefinitionPart;
        }

        /// <summary>
        /// Generate the pair of Task and Resource tables for the specified taskElement.
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="worksheetName"></param>
        /// <param name="workspace"></param>
        /// <param name="addResourceButtonId"></param>
        /// <param name="tablePartDefinitionIds"></param>
        /// <param name="allWorkspaceCustomFields"></param>
        /// <param name="dataValidationReferences"></param>
        /// <param name="rowIndex"></param>
        /// <param name="taskElement"></param>
        /// <param name="newTaskTableName"></param>
        /// <param name="newResourceTableName"></param>
        /// <param name="newTaskTableDefId"></param>
        /// <param name="newResourceTableDefid"></param>
        /// <param name="isFirstTask"></param>
        /// <returns></returns>
        private uint GenerateTaskAndResourceTables(
            SpreadsheetDocument spreadsheet,
            WorkofflineTemplate workofflineTemplate, 
            WorksheetPart worksheetPart, 
            string worksheetName,
            FullWorkspace workspace, 
            uint addResourceButtonId,
            ICollection<string> tablePartDefinitionIds,
            IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, 
            Dictionary<string, string> dataValidationReferences,
            uint rowIndex,
            IReadOnlyCollection<ClinDTO> wsClins,
            IReadOnlyCollection<FullWbs> wsWBSs,
            BoeTaskElementDTO taskElement,
            FullBoe boeForTaskElement,
            string newTaskTableName, 
            string newResourceTableName,
            uint newTaskTableDefId, 
            uint newResourceTableDefid,
            bool isFirstTask)
        {
            // Generate Task data
            if (workofflineTemplate.TaskTable != null)
            {
                TableDefinitionPart taskTableDefinitionPart = this.GenerateTaskWorksheetData(
                    spreadsheet,
                    workofflineTemplate, 
                    worksheetPart, 
                    newTaskTableDefId, 
                    newTaskTableName,
                    rowIndex, 
                    taskElement, 
                    allWorkspaceCustomFields, 
                    dataValidationReferences,
                    workspace);
                tablePartDefinitionIds.Add(worksheetPart.GetIdOfPart(taskTableDefinitionPart));
                TableRange tableRange = new TableRange(worksheetName, taskTableDefinitionPart.Table);
                rowIndex = tableRange.RowEnd + 3;
            }

            // Generate Resource data
            if (workofflineTemplate.ResourceTable != null)
            {
                TableDefinitionPart resourceTableDefinitionPart = this.GenerateResourceWorksheetData(spreadsheet,
                    workofflineTemplate, worksheetPart, newResourceTableDefid, newResourceTableName,
                    rowIndex, taskElement,boeForTaskElement != null ? boeForTaskElement.IsMultiClinWbs:true, wsClins,wsWBSs, allWorkspaceCustomFields, dataValidationReferences, workspace);
                tablePartDefinitionIds.Add(worksheetPart.GetIdOfPart(resourceTableDefinitionPart));

                TableRange tableRange = new TableRange(worksheetName, resourceTableDefinitionPart.Table);
                rowIndex = tableRange.RowEnd;

                // Each Resource table needs an "Add Resource" button.
                // When we clone the template worksheet to create each BOE worksheet we end up with one "Add Task" and one "Add Resource" button.
                // For the first resource table exported, we can move the existing "Add Resource" button to the last row of the table.
                // For all subsequent resource tables in the worksheet, we need to clone the original "Add Resource" button and position it 
                // so it is attached to the last row of the table.
                if (isFirstTask)
                {
                    MoveButton(worksheetPart, addResourceButtonId, 1, rowIndex - 1);    // place in column B of last row in table
                }
                else
                {
                    CloneButton(worksheetPart, addResourceButtonId, 1, rowIndex - 1);   // place in column B of last row in table
                }

                rowIndex = tableRange.RowEnd + 3;
            }
            return rowIndex;
        }

        /// <summary>
        /// Generate a Task table with data
        /// </summary>
        /// <param name="spreadsheet">spreadsheet</param>
        /// <param name="workofflineTemplate">workoffline template</param>
        /// <param name="worksheetPart">worksheed part</param>
        /// <param name="newTableDefPartId">new table def part id</param>
        /// <param name="newTableName">new table name</param>
        /// <param name="rowIndex">row index</param>
        /// <param name="taskElement">task element</param>
        /// <param name="allWorkspaceCustomFields">Collection of custom fields - updated by this method</param>
        /// <param name="dataValidationReferences">Collection of data validation references - updated by this method</param>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>TableDefinitionPart containing new Task table</returns>
        private TableDefinitionPart GenerateTaskWorksheetData(
            SpreadsheetDocument spreadsheet, 
            WorkofflineTemplate workofflineTemplate,
            WorksheetPart worksheetPart, 
            uint newTableDefPartId, 
            string newTableName, 
            uint rowIndex, 
            BoeTaskElementDTO taskElement,
            IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, 
            Dictionary<string, string> dataValidationReferences,
            FullWorkspace workspace)
        {
            uint startRowIndex = rowIndex;
            long startColIndex = 1; // start table in column A (one-based column index)
            string taskTableBegin = TableRange.GetColumnName(startColIndex) + startRowIndex;
            rowIndex = WorkofflineTemplate.CopyTemplateTable(workofflineTemplate.TaskTable, workofflineTemplate.TaskTableRows, worksheetPart, startColIndex, startRowIndex);
            Cell TaskTableBegin = ExcelUtilities.GetCell(worksheetPart, taskTableBegin);

            // Update cell values as needed
            if (taskElement != null)
            {
                // Update cell values as needed
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASKELEMENT_ID_CELL_ROW_OFFSET, ImportExportConstants.TASKELEMENT_ID_CELL_COLUMN_OFFSET,
                    taskElement.Id.ToString(), null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_TYPE_CELL_ROW_OFFSET, ImportExportConstants.TASK_TYPE_CELL_COLUMN_OFFSET,
                    taskElement.TaskElementType.GetDescription(), null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_ID_CELL_ROW_OFFSET, ImportExportConstants.TASK_ID_CELL_COLUMN_OFFSET,
                    taskElement.BOETaskID, null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_TITLE_CELL_ROW_OFFSET, ImportExportConstants.TASK_TITLE_CELL_COLUMN_OFFSET,
                    taskElement.TaskTitle, null, true, false);

                if (workspace.RteOverrides.Contains(RteTemplateSource.TaskDescription))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                        ImportExportConstants.TASK_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.TASK_DESCRIPTION_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT_RTE_TEMPLATES, null, true, false);
                }
                else if (!string.IsNullOrEmpty(taskElement.Description))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                        ImportExportConstants.TASK_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.TASK_DESCRIPTION_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT, null, true, false);
                }

                string startDateDisplay = taskElement.StartDate.HasValue ? taskElement.StartDate.Value.ToString("MM/yyyy") : string.Empty;
                string endDateDisplay = taskElement.EndDate.HasValue ? taskElement.EndDate.Value.ToString("MM/yyyy") : string.Empty;
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_STARTDATE_CELL_ROW_OFFSET, ImportExportConstants.TASK_STARTDATE_CELL_COLUMN_OFFSET,
                    startDateDisplay, null, true, false);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_ENDDATE_CELL_ROW_OFFSET, ImportExportConstants.TASK_ENDDATE_CELL_COLUMN_OFFSET,
                    endDateDisplay, null, true, false);

                var inUseWorkspaceVariables = (from wID in taskElement.WorkspaceVariableIDs
                                              from workspaceVariable in workspace.WorkspaceVariables
                                              where workspaceVariable.Id == wID
                                              select workspaceVariable).ToList();

                taskElement.MOQHoursEquation = Common.MOQ.Parser.UntagVariables(taskElement.MOQHoursEquation, inUseWorkspaceVariables);

                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_MOQHOURSEQU_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQHOURSEQU_CELL_COLUMN_OFFSET,
                    taskElement.MOQHoursEquation, null, true, false);

                if (workspace != null && FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson)
                {
                    // change the Label for EP
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_MOQHOURSEQU_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQHOURSEQU_CELL_COLUMN_OFFSET - 1,
                    "MOQ Equation:", null, true, false);
                }

                string moqTypeName = this._ICommonDataMapper.getMOQTypeName(taskElement.MOQType);
                ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                    ImportExportConstants.TASK_MOQTYPE_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQTYPE_CELL_COLUMN_OFFSET,
                    moqTypeName, null, true, false);

                if (workspace.RteOverrides.Contains(RteTemplateSource.TaskMOQ))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                        ImportExportConstants.TASK_MOQTEXT_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQTEXT_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT_RTE_TEMPLATES, null, true, false);

                }
                else if (!string.IsNullOrEmpty(taskElement.MOQText))
                {
                    ExcelUtilities.SetCellValueByOffset(spreadsheet, worksheetPart.Worksheet, TaskTableBegin,
                        ImportExportConstants.TASK_MOQTEXT_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQTEXT_CELL_COLUMN_OFFSET,
                        ImportExportConstants.PLACEHOLDER_TEXT, null, true, false);
                }
            }

            // Add data validation for MOQ Type field
            uint moqTypeRowIndex = startRowIndex + ImportExportConstants.TASK_MOQTYPE_CELL_ROW_OFFSET;
            AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.MOQ_TYPES,
                startColIndex + ImportExportConstants.TASK_MOQTYPE_CELL_COLUMN_OFFSET, moqTypeRowIndex, moqTypeRowIndex);

            // Display Task custom field labels and values
            rowIndex = startRowIndex + ImportExportConstants.TASK_FIRST_CUSTOMFIELD_ROW_OFFSET;  // reset rowIndex to start of custom fields
            Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
            IDictionary<CustomFieldValueDTO, CustomFieldDTO> laborTaskCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
            if (taskElement != null)
            {
                customFieldValueIdMappings = this._ICustomFieldValueDTODataLoader.GetCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int> { taskElement.Id });
                laborTaskCustomFields = this.GetTaskElementCustomFields(customFieldValueIdMappings, allWorkspaceCustomFields);
            }
            foreach (CustomFieldDTO customFieldForLabel in allWorkspaceCustomFields)
            {
                if (customFieldForLabel.CustomFieldDisplayID == CustomFieldType.TaskDisplay)
                {
                    // set label
                    string customFieldLabel = customFieldForLabel.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customFieldForLabel.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customFieldForLabel.CustomFieldName);

                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, rowIndex, startColIndex + ImportExportConstants.TASK_CUSTOMFIELD_LABEL_CELL_COLUMN_OFFSET,
                       customFieldLabel, workofflineTemplate.TaskCustomFieldNameStyleIndex, true, false);
                    // add data validation for the custom field value cell if not open ended
                    if (!customFieldForLabel.IsOpenEnded)
                    {
                        AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customFieldForLabel.Id,
                            startColIndex + ImportExportConstants.TASK_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET, rowIndex, rowIndex);
                    }

                    // set corresponding value (if there is one)
                    foreach (KeyValuePair<CustomFieldValueDTO, CustomFieldDTO> customFieldAndValue in laborTaskCustomFields)
                    {
                        CustomFieldValueDTO fieldValue = customFieldAndValue.Key;
                        CustomFieldDTO field = customFieldAndValue.Value;

                        if (customFieldForLabel.Id == field.Id)
                        {
                            // set value
                            string customFieldText;
                            if (field.IsOpenEnded)
                            {
                                customFieldText = string.Format("{0}", fieldValue.CustomFieldValueDescription);
                            }
                            else
                            {
                                customFieldText =string.Format("{0} - {1}", fieldValue.CustomFieldValueName, fieldValue.CustomFieldValueDescription);
                            }
                            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, rowIndex, startColIndex + ImportExportConstants.TASK_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET,
                                customFieldText, workofflineTemplate.TaskCustomFieldValueStyleIndex, true, false);
                        }
                    }

                    rowIndex++;
                }
            }

            // Return the new TableDefinitionPart
            long endColIndex = startColIndex + (workofflineTemplate.TaskTable.ColumnEndIndex - workofflineTemplate.TaskTable.ColumnBeginIndex);
            TableDefinitionPart taskTableDefinitionPart =
                WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(workofflineTemplate.TaskTable,
                    worksheetPart, newTableDefPartId, newTableName, startColIndex, startRowIndex, endColIndex, (rowIndex - 1U));
            return taskTableDefinitionPart;
        }


        /// <summary>
        /// Generate a Resource table with data
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="newTableDefPartId"></param>
        /// <param name="newTableName"></param>
        /// <param name="rowIndex"></param>
        /// <param name="workspace"></param>
        /// <param name="taskElement"></param>
        /// <param name="allWorkspaceCustomFields">Collection of custom fields - updated by this method</param>
        /// <param name="dataValidationReferences">Collection of data validation references - updated by this method</param>
        /// <returns>TableDefinitionPart containing new Resource table</returns>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private TableDefinitionPart GenerateResourceWorksheetData(SpreadsheetDocument spreadsheet,
            WorkofflineTemplate workofflineTemplate, WorksheetPart worksheetPart, uint newTableDefPartId, string newTableName, uint rowIndex,
            BoeTaskElementDTO taskElement, Boolean isMultiBOE, IReadOnlyCollection<ClinDTO> wsClins, IReadOnlyCollection<FullWbs> wsWBSs, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields,
            Dictionary<string, string> dataValidationReferences, WorkspaceDTO workspace)
        {
            uint startRowIndex = rowIndex;
            long startColIndex = 1; // start table in column A (one-based column index)
            rowIndex = WorkofflineTemplate.CopyTemplateTable(workofflineTemplate.ResourceTable, workofflineTemplate.ResourceTableRows, worksheetPart, startColIndex, startRowIndex);
            
            // Determine where to start adding dynamic column headers
            long endColIndex = startColIndex + ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET + 1;

            if (workspace != null && FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson)
            {
                // change the Label for EP
                ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 1, endColIndex - 2, "EPs Spread", workofflineTemplate.ResourceCustomHeaderStyleIndex, true, false);
            }

            // add dynamic column headers for custom fields
            Dictionary<int, long> allCustomFieldColumnIndices = new Dictionary<int, long>(); // map CustomFieldId to column index
            Dictionary<int, long> nonOpenEndedCustomFieldColumnIndices = new Dictionary<int, long>(); // map CustomFieldId to column index

            foreach (CustomFieldDTO customField in allWorkspaceCustomFields)
            {
                if (customField.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                {
                    string customFieldLabel = customField.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName);
                    allCustomFieldColumnIndices.Add(customField.Id, endColIndex);
                    if(!customField.IsOpenEnded)
                    {
                        nonOpenEndedCustomFieldColumnIndices.Add(customField.Id, endColIndex);
                    }

                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 0, endColIndex, string.Empty, workofflineTemplate.ResourceBannerCellStyle, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 1, endColIndex, customFieldLabel, workofflineTemplate.ResourceCustomHeaderStyleIndex, true, false);
                    endColIndex++;
                }
            }

           
            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 0, endColIndex, string.Empty, workofflineTemplate.ResourceBannerCellStyle, true, false);
            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 1, endColIndex, "WBS", workofflineTemplate.ResourceCustomHeaderStyleIndex, true, false);
            endColIndex++;
            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 0, endColIndex, string.Empty, workofflineTemplate.ResourceBannerCellStyle, true, false);
            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 1, endColIndex, "CLIN", workofflineTemplate.ResourceCustomHeaderStyleIndex, true, false);
            endColIndex++;
            

            // Get Task Resource custom fields and values
            Dictionary<int, ICollection<KeyValuePair<int, int>>> laborTypeCustomFieldValueIdMappings = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
            IDictionary<CustomFieldValueDTO, CustomFieldDTO> allTaskResourcesCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();
            if (taskElement != null)
            {
                laborTypeCustomFieldValueIdMappings = this._ICustomFieldValueDTODataLoader.GetLaborTypeCustomFieldValueIDsContainerIDsByTaskElementIDs(new Collection<int> { taskElement.Id });
                allTaskResourcesCustomFields = this.GetTaskResourceCustomFields(laborTypeCustomFieldValueIdMappings, allWorkspaceCustomFields);
            }

            // add dynamic column headers for monthly spread data
            Dictionary<DateTime, long> spreadDateColumnIndices = new Dictionary<DateTime, long>();    // map spread date to column index
            if (taskElement != null)
            {
                if (taskElement.StartDate.HasValue && taskElement.EndDate.HasValue)
                {
                    // create a lookup table to determine column index for each spread month
                    
                    // We want to export even "bad" data.. An example would be a task with resources that start/end before/after the task does.
                    // So we need to figure out the minimum and maximum dates within the labor task, and then use those when exporting the data..
                    DateTime startingDate = taskElement.taskElementLabors.Where(x => x.StartDate.HasValue).Select(x => x.StartDate.Value)
                                .Union(taskElement.taskElementLabors.SelectMany(x => x.LaborSpreads.Select(z => z.LaborSpreadDate)))
                                .Union(new List<DateTime>() { taskElement.StartDate.Value })
                            .Min();

                    DateTime endingDate = taskElement.taskElementLabors.Where(x => x.EndDate.HasValue).Select(x => x.EndDate.Value)
                                .Union(taskElement.taskElementLabors.SelectMany(x => x.LaborSpreads.Select(z => z.LaborSpreadDate)))
                                .Union(new List<DateTime>() { taskElement.EndDate.Value })
                            .Max();

                    for (DateTime dt = startingDate; dt <= endingDate; dt = dt.AddMonths(1))
                    {
                        // use new DateTime objects based on Year/Month/Day only (no time components)
                        spreadDateColumnIndices.Add(new DateTime(dt.Year, dt.Month, dt.Day), endColIndex);

                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 0, endColIndex, string.Empty, workofflineTemplate.ResourceBannerCellStyle, true, false);
                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, startRowIndex + 1, endColIndex, dt.ToString("MM/yyyy"), workofflineTemplate.ResourceHoursHeaderStyleIndex, true, false);
                        endColIndex++;
                    }
                }
            }

            // Update cell values for each resource row
            Collection<SpreadCurveModelView> allCurves = this._ICommonDataMapper.getSpreadCurve();
            uint startDataRowIndex = startRowIndex + ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET;    // start resource data after the header rows
            uint curDataRowIndex = startDataRowIndex;

            // process all resources for the current task element
            Collection<ResourceTypeDto> resources = taskElement != null ? taskElement.taskElementLabors : new Collection<ResourceTypeDto>();

            if (resources.Any())
            {
                HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this._IResourceDTODataLoader.GetByIds(taskElement.taskElementLabors.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
                HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(resources.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

                foreach (ResourceTypeDto resource in resources.OrderBy(x => x.LaborTypeOrder).ThenBy(x => x.Id))
                {
                    string resourceName = string.Empty;
                    if (resource.ResourceID.HasValue)
                    {
                        ResourceDTO resourceDto = resourcesFromDb.First(x => x.Id == resource.ResourceID.Value);
                        resourceName = resourceDto.ResourceDesc;
                    }

                    string performingOrgName = string.Empty;
                    if (resource.PerformingOrgID.HasValue)
                    {
                        PerformingOrgDTO performingOrgDto = perfOrgsFromDb.First(x => x.Id == resource.PerformingOrgID.Value);
                        performingOrgName = string.Format("{0} - {1}", performingOrgDto.PerformingOrgName, performingOrgDto.PerformingOrgDesc);
                    }

                    string startDateDisplay = resource.StartDate.HasValue ? resource.StartDate.Value.ToString("MM/yyyy") : string.Empty;
                    string endDateDisplay = resource.EndDate.HasValue ? resource.EndDate.Value.ToString("MM/yyyy") : string.Empty;

                    SpreadCurveModelView thisSpread = (from curves in allCurves where curves.SpreadCurveID == resource.SpreadCurveID select curves).FirstOrDefault();

                    /*
                     * Column  Contents
                     * ------  ---------------
                     *   A     Resource ID
                     *   B     Resource
                     *   C     Performing Org
                     *   D     Start Date
                     *   E     End Date
                     *   F     Spread Curve
                     *   G     Percent Spread
                     *   H     Hour Amount Spread
                     *   I     Cost
                     *  J-?    Optional Custom Fields and Spread data
                     * 
                     */
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_COLUMN_OFFSET,
                        resource.Id.ToString(), null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET,
                        resourceName, null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET,
                        performingOrgName, null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_STARTDATE_CELL_COLUMN_OFFSET,
                        startDateDisplay, null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_ENDDATE_CELL_COLUMN_OFFSET,
                        endDateDisplay, null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_SPREADCURVE_CELL_COLUMN_OFFSET,
                        thisSpread.SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(workspace)), null, true, false);
                    ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_PERCENTSPREAD_CELL_COLUMN_OFFSET,
                        resource.PercentSpread.HasValue ? resource.PercentSpread.Value.ToString("F3") : "0", null, true, false);
                   
                    if (resource.SpreadType != SpreadType.Cost)
                    {
                        // Export Hours Value
                        string valueSpreadDisplayValue = Utilities.FormatStringWithPrecisionNoComma((resource.ValueSpread.HasValue ? resource.ValueSpread.Value : 0), workspace.DecimalPrecision);
                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_HOURSSPREAD_CELL_COLUMN_OFFSET,
                             valueSpreadDisplayValue, null, true, false);

                    }
                    else
                    {                     
                        string costValue = Utilities.FormatStringWithPrecisionNoComma((resource.ValueSpread.HasValue ? resource.ValueSpread.Value : 0), 2);
                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, startColIndex + ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET,
                                costValue, null, true, false);
                    }
              

                    // Display custom fields (if any)
                    int laborTypeId = resource.Id;
                    if (laborTypeCustomFieldValueIdMappings.ContainsKey(laborTypeId))
                    {
                        ICollection<int> laborTypeCustomFieldValueIds = laborTypeCustomFieldValueIdMappings[laborTypeId].Select(c => c.Value).ToList();
                        var laborTypeCustomFields = from cf in allTaskResourcesCustomFields
                                                    join id in laborTypeCustomFieldValueIds on cf.Key.CustomFieldValueID equals id
                                                    select new
                                                    {
                                                        CustomFieldValueID = cf.Key.CustomFieldValueID,
                                                        CustomFieldId = cf.Value.Id,
                                                        CustomFieldName = cf.Value.CustomFieldName,
                                                        CustomFieldValueName = cf.Key.CustomFieldValueName,
                                                        CustomFieldValueDescription = cf.Key.CustomFieldValueDescription,
                                                        IsOpenEnded = cf.Value.IsOpenEnded
                                                    };

                        foreach (var laborTypeCustomField in laborTypeCustomFields)
                        {
                            string customFieldText;
                            if(laborTypeCustomField.IsOpenEnded)
                            {
                                customFieldText = string.Format("{0}", laborTypeCustomField.CustomFieldValueDescription);
                            }
                            else
                            {
                                customFieldText = string.Format("{0} - {1}", laborTypeCustomField.CustomFieldValueName, laborTypeCustomField.CustomFieldValueDescription);
                            }
                            ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, allCustomFieldColumnIndices[laborTypeCustomField.CustomFieldId],
                                customFieldText, workofflineTemplate.ResourceCustomStyleIndex, true, false);
                        }
                    }
                    if (isMultiBOE)
                    {
                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, (startColIndex + ImportExportConstants.RESOURCETYPE_WBS_CELL_COLUMN_OFFSET + (allCustomFieldColumnIndices.Count)),
                            resource.WBSID.HasValue ? wsWBSs.FirstOrDefault(w => w.Id == resource.WBSID.Value).WbsString : "", null, true, false);
                        ExcelUtilities.SetCellValue(spreadsheet, worksheetPart.Worksheet, curDataRowIndex, (startColIndex + ImportExportConstants.RESOURCETYPE_CLIN_CELL_COLUMN_OFFSET + (allCustomFieldColumnIndices.Count)),
                            resource.CLINID.HasValue ? wsClins.FirstOrDefault(c => c.Id == resource.CLINID.Value).ClinString : "", null, true, false);
                    }

                    // Display month-by-month values for current resource row (export discrete hours/costs, but not curve values)
                    if (thisSpread.SpreadCurveID == SpreadCurves.DiscreteCost ||
                        thisSpread.SpreadCurveID == SpreadCurves.DiscreteHours)
                    {
                        foreach (ResourceSpreadDto laborSpread in resource.LaborSpreads)
                        {
                            // store current LaborSpread month into new DateTime object based on Year/Month/Day only (no time components)
                            DateTime spreadMonth = new DateTime(laborSpread.LaborSpreadDate.Year, laborSpread.LaborSpreadDate.Month, laborSpread.LaborSpreadDate.Day);
                            if (resource.SpreadType == SpreadType.Cost)
                            {
                                string laborSpreadDisplayValue = (laborSpread.LaborSpreadValue).ToString("F2");
                                ExcelUtilities.SetCellValue(worksheetPart.Worksheet, curDataRowIndex, spreadDateColumnIndices[spreadMonth], CellValues.Number,
                                    laborSpreadDisplayValue, workofflineTemplate.ResourceCostStyleIndex, false);
                            }
                            else
                            {
                                string laborSpreadDisplayValue = Utilities.FormatStringWithPrecisionNoComma(laborSpread.LaborSpreadValue, workspace.DecimalPrecision);
                                ExcelUtilities.SetCellValue(worksheetPart.Worksheet, curDataRowIndex, spreadDateColumnIndices[spreadMonth], CellValues.String,
                                    laborSpreadDisplayValue, workofflineTemplate.ResourceHoursStyleIndex, false);
                            }
                        }
                    }
                    curDataRowIndex++;
                }
            }
            else
            {
                // No resources for this task.  
                if (taskElement != null)
                {
                    // Set resource ID value in column A of first (empty) resource data row so it is ready for user input.
                    ExcelUtilities.SetCellValue(worksheetPart.Worksheet, curDataRowIndex, 1, CellValues.Number, workofflineTemplate.DecrementResourceIdCounter().ToString(), null, false);
                }
            }

            // Update data validation references for Resource, Performing Org, and Spread Curve columns
            if (startDataRowIndex == curDataRowIndex)
            {
                curDataRowIndex++;    // no resources exported, but still need data validation on first row from template
            }
            AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCES,
                (startColIndex + ImportExportConstants.RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET), startDataRowIndex, (curDataRowIndex - 1U));
            AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.PERFORGS,
                (startColIndex + ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET), startDataRowIndex, (curDataRowIndex - 1U));
            AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.SPREAD_CURVES,
                (startColIndex + ImportExportConstants.RESOURCETYPE_SPREADCURVE_CELL_COLUMN_OFFSET), startDataRowIndex, (curDataRowIndex - 1U));
           
            // Update data validation references for each custom field column that isn't open ended
            foreach (KeyValuePair<int, long> customFieldColumnIndex in nonOpenEndedCustomFieldColumnIndices)
            {
                AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customFieldColumnIndex.Key,
                    customFieldColumnIndex.Value, startDataRowIndex, (curDataRowIndex - 1U));
            }

            if (isMultiBOE)
            {
                AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.WBSS,
             startColIndex + ImportExportConstants.RESOURCETYPE_WBS_CELL_COLUMN_OFFSET + (allCustomFieldColumnIndices.Count), startDataRowIndex, (curDataRowIndex - 1U));
                AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CLINS,
                 startColIndex + ImportExportConstants.RESOURCETYPE_CLIN_CELL_COLUMN_OFFSET + (allCustomFieldColumnIndices.Count), startDataRowIndex, (curDataRowIndex - 1U));
            }

            // Return the new TableDefinitionPart
            rowIndex = Math.Max(rowIndex, (curDataRowIndex + 1U));  // allow for blank row at bottom of table
            TableDefinitionPart resourceTableDefinitionPart =
                WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(workofflineTemplate.ResourceTable,
                    worksheetPart, newTableDefPartId, newTableName, startColIndex, startRowIndex, (endColIndex - 1), (rowIndex - 1U));
            return resourceTableDefinitionPart;
        }

        /// <summary>
        /// Regenerate the Dynamic Columns Template table in a new location -- below the resource table template.
        /// </summary>
        /// <param name="workofflineTemplate"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="tablePartDefinitionIds"></param>
        /// <param name="newTableDefId"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private uint RegenerateDynamicColumnsTemplate(WorkofflineTemplate workofflineTemplate, WorksheetPart worksheetPart, Collection<string> tablePartDefinitionIds,
            uint newTableDefId, uint rowIndex)
        {
            long newColBeginIndex = workofflineTemplate.DynamicColumnsTable.ColumnBeginIndex;
            long newColEndIndex = workofflineTemplate.DynamicColumnsTable.ColumnEndIndex;
            uint newRowBeginIndex = rowIndex;
            uint newRowEndIndex = newRowBeginIndex + (workofflineTemplate.DynamicColumnsTable.RowEnd - workofflineTemplate.DynamicColumnsTable.RowBegin);

            // Re-generate the Dynamic Columns template (reposition so it is beyond the new resource template table with custom fields)
            WorkofflineTemplate.CopyTemplateTable(workofflineTemplate.DynamicColumnsTable, workofflineTemplate.DynamicColumnsRows, worksheetPart, newColBeginIndex, newRowBeginIndex);
            TableDefinitionPart dynamicColumnsTableDefinitionPart =
                WorkofflineTemplate.GenerateTableDefinitionPartFromTemplate(workofflineTemplate.WorkspaceTable,
                    worksheetPart, newTableDefId, ImportExportConstants.DYNAMIC_COLUMNS_TEMPLATE, newColBeginIndex, newRowBeginIndex, newColEndIndex, newRowEndIndex);
            tablePartDefinitionIds.Add(worksheetPart.GetIdOfPart(dynamicColumnsTableDefinitionPart));

            return rowIndex;
        }

        #region Data Validation
        /// <summary>
        /// Add the specified definedName/cell reference to the data validations set.
        /// 
        /// Notes:
        /// OpenXML DataValidation elements map a defined name to a sequence of cell references
        /// separated by spaces, e.g. A3 A10 B30:B35 A22.
        /// Each reference may be a single cell or a range of cells.
        /// 
        /// We use dataValidationReferences dictionary to map each defined name to the corresponding sequence of references.
        /// For example:
        ///     DefinedName     =>  SequenceOfReferences
        ///     ----------------------------------------
        ///     MOQType         =>  C15 C42 C73
        ///     CustomField_111 =>  C20 C47 C78
        ///     Resource        =>  B28:B30 B46:B48
        ///     PerfOrgs        =>  C28:C30 C46:C48
        ///     SpreadCurve     =>  F28:F30 F46:F48
        ///     CustomField_222 =>  I28:I30 I46:I48
        ///     CustomField_333 =>  J28:J30 J46:J48
        /// </summary>
        /// <param name="definedName"></param>
        /// <param name="colIndex"></param>
        /// <param name="startRowIndex"></param>
        /// <param name="endRowIndex"></param>
        /// <param name="dataValidationReferences"></param>
        private static void AddCellReferenceToDataValidationDictionary(Dictionary<string, string> dataValidationReferences,
            string definedName, long colIndex, uint startRowIndex, uint endRowIndex)
        {
            // add cell reference to data validation map
            string cellRange = (startRowIndex == endRowIndex) ?
                TableRange.GetColumnName(colIndex) + startRowIndex :
                TableRange.GetColumnName(colIndex) + startRowIndex + ":" + TableRange.GetColumnName(colIndex) + endRowIndex;
            if (dataValidationReferences.ContainsKey(definedName))
            {
                // append new cell range to existing value(s)
                dataValidationReferences[definedName] = dataValidationReferences[definedName] + " " + cellRange;
            }
            else
            {
                // add new cell range
                dataValidationReferences.Add(definedName, cellRange);
            }
        }

        /// <summary>
        /// Create DataValidation elements for MOQ Type, Resource, Performing Org, Spread Curve, and custom fields
        /// contained in the dataValidationReferences dictionary.
        /// </summary>
        /// <param name="dataValidationReferences"></param>
        /// <returns></returns>
        private static DataValidations GenerateDataValidations(Dictionary<string, string> dataValidationReferences)
        {
            // 
            DataValidations dataValidations = new DataValidations() { Count = Convert.ToUInt32(dataValidationReferences.Count) };

            // Add data validations for each defined name
            foreach (KeyValuePair<string, string> dataValidationReference in dataValidationReferences)
            {
                DataValidation dataValidation = new DataValidation()
                {
                    Type = DataValidationValues.List,
                    AllowBlank = true,
                    ShowInputMessage = true,
                    ShowErrorMessage = true,
                    SequenceOfReferences = new ListValue<StringValue>() { InnerText = dataValidationReference.Value.Trim() }
                };
                Formula1 formula1 = new Formula1();
                formula1.Text = dataValidationReference.Key;    // defined name
                dataValidation.Append(formula1);
                dataValidations.Append(dataValidation);
            }
            return dataValidations;
        }
        #endregion

        #region Custom Fields
        /// <summary>
        /// Get the Task Element Custom Fields and associated values.
        /// </summary>
        /// <param name="customFieldValueIdMappings"></param>
        /// <param name="workspaceCustomFields"></param>
        /// <returns></returns>
        private IDictionary<CustomFieldValueDTO, CustomFieldDTO> GetTaskElementCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings, IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields)
        {
            IDictionary<CustomFieldValueDTO, CustomFieldDTO> taskElementCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();

            foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> laborTaskCustomFieldIdMapping in customFieldValueIdMappings)
            {
                ICollection<KeyValuePair<int, int>> idPairs = laborTaskCustomFieldIdMapping.Value;

                // Get all custom field values for the task element.
                ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
                ICollection<CustomFieldValueDTO> customFieldValues = this._ICustomFieldValueDTODataLoader.GetByIds(customFieldValueIds);

                foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues)
                {
                    CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
                    if (customFieldDto != null)
                    {
                        // add to list
                        taskElementCustomFields.Add(customFieldValueDto, customFieldDto);
                    }
                }
            }

            return taskElementCustomFields;
        }

        /// <summary>
        /// Get the Task Resource Custom Fields and associated values.
        /// </summary>
        /// <param name="customFieldValueIdMappings"></param>
        /// <param name="workspaceCustomFields"></param>
        /// <returns></returns>
        private IDictionary<CustomFieldValueDTO, CustomFieldDTO> GetTaskResourceCustomFields(Dictionary<int, ICollection<KeyValuePair<int, int>>> customFieldValueIdMappings, IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields)
        {
            IDictionary<CustomFieldValueDTO, CustomFieldDTO> taskResourceElementCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();

            foreach (KeyValuePair<int, ICollection<KeyValuePair<int, int>>> taskResourceCustomFieldIdMapping in customFieldValueIdMappings)
            {
                ICollection<KeyValuePair<int, int>> idPairs = taskResourceCustomFieldIdMapping.Value;

                // Get all custom field values for the task element.
                ICollection<int> customFieldValueIds = idPairs.Select(i => i.Value).Distinct().ToCollection<int>();
                ICollection<CustomFieldValueDTO> customFieldValues = this._ICustomFieldValueDTODataLoader.GetByIds(customFieldValueIds);

                foreach (CustomFieldValueDTO customFieldValueDto in customFieldValues)
                {
                    CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
                    if (customFieldDto != null)
                    {
                        // add to list
                        taskResourceElementCustomFields.Add(customFieldValueDto, customFieldDto);
                    }
                }
            }

            return taskResourceElementCustomFields;
        }
        #endregion

        #region Button Methods

        /// <summary>
        /// Searches the worksheetPart for the first button with the specified name.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private static uint FindButtonByName(WorksheetPart worksheetPart, string buttonName)
        {
            uint buttonId = 0;

            foreach (AlternateContent wsDrAlternateContent in worksheetPart.DrawingsPart.WorksheetDrawing.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice alternateContentChoice in wsDrAlternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (TwoCellAnchor twoCellAnchor in alternateContentChoice.Elements<TwoCellAnchor>())
                    {
                        foreach (Shape shape in twoCellAnchor.Elements<Shape>())
                        {
                            string currentButtonName = shape.TextBody.Elements<A.Paragraph>().First().InnerText;

                            // Does current button name match?
                            if (currentButtonName.Equals(buttonName))
                            {
                                buttonId = shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id;
                                return buttonId;
                            }
                        }
                    }
                }
            }

            return buttonId;
        }

        /// <summary>
        /// Move a button to the specified location.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToMoveId"></param>
        /// <param name="toColIndex"></param>
        /// <param name="toRowIndex"></param>
        private static void MoveButton(WorksheetPart worksheetPart, uint buttonToMoveId, int toColIndex, uint toRowIndex)
        {
            // When updating or copying a button, we need to work with 3 XML components:
            //  Component                               Excel package location
            //  --------------------------------------  ---------------------------------------------------
            //  1) VmlDrawing,                          /xl/drawings/vmldrawing1.vml, vmldrawing2.vml, etc.
            //  2) worksheet drawing AlternateContent,  /xl/drawings/drawing1.xml, drawing2.xml, etc.
            //  3) worksheet AlternateContent           /xl/worksheets/sheet1.xml, sheet2.xml, etc.
            if (buttonToMoveId > 0)
            {
                // Update button location
                UpdateVmlDrawing(worksheetPart, buttonToMoveId, toColIndex, toRowIndex);
                UpdateWsDrAlternateContent(worksheetPart, buttonToMoveId, toColIndex, toRowIndex);
                UpdateWsAlternateContent(worksheetPart, buttonToMoveId, toColIndex, toRowIndex);
            }
        }

        /// <summary>
        /// Update the position of a shape (button) within the VmlDrawing element.
        /// The following is a snippet of XML from /xl/drawings/vmlDrawing.vml:
        ///      <xml xmlns:v="urn:schemas-microsoft-com:vml"
        ///       xmlns:o="urn:schemas-microsoft-com:office:office"
        ///       xmlns:x="urn:schemas-microsoft-com:office:excel">
        ///       <o:shapelayout v:ext="edit">
        ///        <o:idmap v:ext="edit" data="31"/>
        ///       </o:shapelayout><v:shapetype id="_x0000_t201" coordsize="21600,21600" o:spt="201"
        ///        path="m,l,21600r21600,l21600,xe">
        ///        <v:stroke joinstyle="miter"/>
        ///        <v:path shadowok="f" o:extrusionok="f" strokeok="f" fillok="f" o:connecttype="rect"/>
        ///        <o:lock v:ext="edit" shapetype="t"/>
        ///       </v:shapetype><v:shape id="_x0000_s31747" type="#_x0000_t201" style='position:absolute;
        ///        margin-left:232.5pt;margin-top:36pt;width:88.5pt;height:18.75pt;z-index:1;
        ///        mso-wrap-style:tight' o:button="t" fillcolor="buttonFace [67]" strokecolor="windowText [64]"
        ///        o:insetmode="auto">
        ///        <v:fill color2="buttonFace [67]" o:detectmouseclick="t"/>
        ///        <o:lock v:ext="edit" rotation="t"/>
        ///        <v:textbox style='mso-direction-alt:auto' o:singleclick="f">
        ///         <div style='text-align:center'><font face="Calibri" size="260"
        ///         color="#000000"><b>Add New Task</b></font><font face="Calibri" size="260"
        ///         color="#000000"> </font></div>
        ///        </v:textbox>
        ///        <x:ClientData ObjectType="Button">
        ///         <x:SizeWithCells/>
        ///         <x:Anchor>1, 171, 2, 8, 2, 66, 3, 13</x:Anchor>
        ///         <x:PrintObject>False</x:PrintObject>
        ///         <x:AutoFill>False</x:AutoFill>
        ///         <x:FmlaMacro>[0]!AddTaskAndResourceTables</x:FmlaMacro>
        ///         <x:TextHAlign>Center</x:TextHAlign>
        ///         <x:TextVAlign>Center</x:TextVAlign>
        ///        </x:ClientData>
        ///       </v:shape>
        ///       <v:shape id="_x0000_s31746" type="#_x0000_t201" style='position:absolute;
        ///        margin-left:116.25pt;margin-top:693pt;width:88.5pt;height:18.75pt;z-index:2;
        ///        mso-wrap-style:tight' o:gfxdata="blah blah blah"
        ///        o:button="t" fillcolor="buttonFace [67]" strokecolor="windowText [64]"
        ///        o:insetmode="auto">
        ///        <v:fill color2="buttonFace [67]" o:detectmouseclick="t"/>
        ///        <o:lock v:ext="edit" rotation="t"/>
        ///        <v:textbox style='mso-direction-alt:auto' o:singleclick="f">
        ///         <div style='text-align:center'><font face="Calibri" size="260"
        ///         color="#000000"><b>Add Resource</b></font><font face="Calibri" size="260"
        ///         color="#000000"> </font></div>
        ///        </v:textbox>
        ///        <x:ClientData ObjectType="Button">
        ///         <x:SizeWithCells/>
        ///         <x:Anchor>1, 16, 30, 4, 1, 134, 31, 9</x:Anchor>
        ///         <x:PrintObject>False</x:PrintObject>
        ///         <x:AutoFill>False</x:AutoFill>
        ///         <x:FmlaMacro>[0]!AddNewResourceRow</x:FmlaMacro>
        ///         <x:TextHAlign>Center</x:TextHAlign>
        ///         <x:TextVAlign>Center</x:TextVAlign>
        ///        </x:ClientData>
        ///       </v:shape>
        ///       <v:shape id="Button_x0020_28675" o:spid="_x0000_s31747" type="#_x0000_t201"
        ///        style='position:absolute;margin-left:116.25pt;margin-top:1098pt;width:88.5pt;
        ///        height:18.75pt;z-index:6;mso-wrap-style:tight' fillcolor="buttonFace [67]"
        ///        o:insetmode="auto">
        ///        <v:fill color2="buttonFace [67]" o:detectmouseclick="t"/>
        ///        <o:lock v:ext="edit" rotation="t"/>
        ///        <v:textbox style='mso-direction-alt:auto' o:singleclick="f">
        ///         <div style='text-align:center'><font face="Calibri" size="260"
        ///         color="#000000"><b>Add Resource</b></font></div>
        ///        </v:textbox>
        ///        <x:ClientData ObjectType="Button">
        ///         <x:SizeWithCells/>
        ///         <x:Anchor>1, 16, 49, 4, 1, 134, 50, 9</x:Anchor>
        ///         <x:PrintObject>False</x:PrintObject>
        ///         <x:AutoFill>False</x:AutoFill>
        ///         <x:FmlaMacro>[0]!AddNewResourceRow</x:FmlaMacro>
        ///         <x:TextHAlign>Center</x:TextHAlign>
        ///         <x:TextVAlign>Center</x:TextVAlign>
        ///        </x:ClientData>
        ///       </v:shape></xml>
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToUpdateId"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        private static void UpdateVmlDrawing(WorksheetPart worksheetPart, uint buttonToUpdateId, int colIndex, uint rowIndex)
        {
            // Update VmlDrawingParts
            foreach (VmlDrawingPart vmlDrawingPart in worksheetPart.VmlDrawingParts)
            {
                //Parse the vmlDrawingPart XML
                XmlDocument xmldoc = new XmlDocument();
                using (Stream stream = vmlDrawingPart.GetStream())
                {
                    xmldoc.Load(stream);
                }


                //Instantiate an XmlNamespaceManager object. 
                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmldoc.NameTable);

                //Add the namespaces used in vmlDrawingPart to the XmlNamespaceManager.
                xmlnsManager.AddNamespace("v", "urn:schemas-microsoft-com:vml");
                xmlnsManager.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
                xmlnsManager.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");

                //Search for shape node with matching ID
                string shapeId = "_x0000_s" + buttonToUpdateId;
                XmlNodeList shapeNodes = xmldoc.SelectNodes("//v:shape[@id='" + shapeId + "']", xmlnsManager);
                foreach (XmlNode shapeNode in shapeNodes)
                {
                    XmlNode clientData = shapeNode.SelectSingleNode("x:ClientData", xmlnsManager);
                    XmlNode anchor = clientData.SelectSingleNode("x:Anchor", xmlnsManager);
                    foreach (XmlNode anchorText in anchor.ChildNodes)
                    {
                        // Process the anchor element values as follows:
                        //  <x:Anchor>fromColId, fromColOffset, fromRowId, fromRowOffset, toColId, toColOffset, toRowId, toRowOffset</x:Anchor>
                        // For example:
                        //  <x:Anchor>1, 171, 2, 8, 2, 66, 3, 13</x:Anchor>
                        string value = anchorText.Value.Trim(); // remove any whitespace, e.g. \r\n
                        string[] tokens = value.Split(',');
                        int fromColId, toColId;
                        uint fromRowId, toRowId;
                        if (int.TryParse(tokens[0], out fromColId) &&
                            uint.TryParse(tokens[2], out fromRowId) &&
                            int.TryParse(tokens[4], out toColId) &&
                            uint.TryParse(tokens[6], out toRowId))
                        {
                            // Adjust the row/column Ids (leave row/column offsets unchanged)
                            int numCols = toColId - fromColId;
                            uint numRows = toRowId - fromRowId;
                            tokens[0] = (colIndex).ToString();
                            tokens[2] = (rowIndex).ToString();
                            tokens[4] = (colIndex + numCols).ToString();
                            tokens[6] = (rowIndex + numRows).ToString();

                            value = string.Join(",", tokens);
                            anchorText.Value = value;
                        }
                    }
                }

                //Store the updated vmlDrawingPart
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmldoc.Save(memoryStream);
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    StreamReader sr = new StreamReader(memoryStream);
                    string vmlDrawingPartString = sr.ReadToEnd();
                    GenerateVmlDrawingPartContent(vmlDrawingPart, vmlDrawingPartString);
                }
            }
        }

        /// <summary>
        /// Update the position of the twoCellAnchor element (button location) within the Worksheet Drawing AlternateContent element.
        /// The following is a snippet of XML from /xl/drawings/drawing.xml:
        ///      <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        ///      <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
        ///        <mc:AlternateContent xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
        ///          <mc:Choice xmlns:a14="http://schemas.microsoft.com/office/drawing/2010/main" Requires="a14">
        ///            <xdr:twoCellAnchor editAs="oneCell">
        ///              <xdr:from>
        ///                <xdr:col>1</xdr:col>
        ///                <xdr:colOff>152400</xdr:colOff>
        ///                <xdr:row>30</xdr:row>
        ///                <xdr:rowOff>38100</xdr:rowOff>
        ///              </xdr:from>
        ///              <xdr:to>
        ///                <xdr:col>1</xdr:col>
        ///                <xdr:colOff>1276350</xdr:colOff>
        ///                <xdr:row>31</xdr:row>
        ///               <xdr:rowOff>85725</xdr:rowOff>
        ///              </xdr:to>
        ///             <xdr:sp macro="" textlink="">
        ///               <xdr:nvSpPr>
        ///                 <xdr:cNvPr id="31745" name="Button 1" hidden="1">
        ///                   <a:extLst>
        ///                      <a:ext uri="{63B3BB69-23CF-44E3-9099-C40C66FF867C}">
        ///                        <a14:compatExt spid="_x0000_s31745"/>
        ///                      </a:ext>
        ///                    </a:extLst>
        ///                  </xdr:cNvPr>
        ///                  <xdr:cNvSpPr/>
        ///                </xdr:nvSpPr>
        ///                <xdr:spPr> ... </xdr:spPr>
        ///                <xdr:txBody> ... </xdr:txBody>
        ///              </xdr:sp>
        ///              <xdr:clientData fPrintsWithSheet="0"/>
        ///            </xdr:twoCellAnchor>
        ///          </mc:Choice>
        ///          <mc:Fallback/>
        ///        </mc:AlternateContent>
        ///        <mc:AlternateContent xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
        ///          <mc:Choice xmlns:a14="http://schemas.microsoft.com/office/drawing/2010/main" Requires="a14">
        ///            <xdr:twoCellAnchor editAs="oneCell">
        ///              <xdr:from>
        ///                <xdr:col>1</xdr:col>
        ///                <xdr:colOff>1628775</xdr:colOff>
        ///                <xdr:row>2</xdr:row>
        ///                <xdr:rowOff>76200</xdr:rowOff>
        ///              </xdr:from>
        ///              <xdr:to>
        ///               <xdr:col>2</xdr:col>
        ///               <xdr:colOff>628650</xdr:colOff>
        ///               <xdr:row>3</xdr:row>
        ///                <xdr:rowOff>123825</xdr:rowOff>
        ///              </xdr:to>
        ///              <xdr:sp macro="" textlink="">
        ///                <xdr:nvSpPr>
        ///                 <xdr:cNvPr id="31746" name="Button 2" hidden="1">
        ///                   <a:extLst>
        ///                     <a:ext uri="{63B3BB69-23CF-44E3-9099-C40C66FF867C}">
        ///                       <a14:compatExt spid="_x0000_s31746"/>
        ///                     </a:ext>
        ///                    </a:extLst>
        ///                  </xdr:cNvPr>
        ///                  <xdr:cNvSpPr/>
        ///                </xdr:nvSpPr>
        ///               <xdr:spPr> ... </xdr:spPr>
        ///               <xdr:txBody> ... </xdr:txBody>
        ///              </xdr:sp>
        ///             <xdr:clientData fPrintsWithSheet="0"/>
        ///           </xdr:twoCellAnchor>
        ///          </mc:Choice>
        ///         <mc:Fallback/>
        ///       </mc:AlternateContent>
        ///      </xdr:wsDr>
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToUpdateId"></param>
        /// <param name="colIndexAdjustment"></param>
        /// <param name="rowIndex"></param>
        private static void UpdateWsDrAlternateContent(WorksheetPart worksheetPart, uint buttonToUpdateId, int colIndex, uint rowIndex)
        {
            // Update Drawing Parts
            foreach (AlternateContent alternateContent in worksheetPart.DrawingsPart.WorksheetDrawing.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (TwoCellAnchor twoCellAnchor in alternateContentChoice.Elements<TwoCellAnchor>())
                    {
                        // Is this the anchor/shape we want to update?
                        foreach (Shape shape in twoCellAnchor.Elements<Shape>())
                        {
                            if (shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id == buttonToUpdateId)
                            {
                                // Update column position
                                int fromColumnId, toColumnId;
                                if (int.TryParse(twoCellAnchor.FromMarker.ColumnId.Text, out fromColumnId) &&
                                    int.TryParse(twoCellAnchor.ToMarker.ColumnId.Text, out toColumnId))
                                {
                                    twoCellAnchor.FromMarker.ColumnId.Text = (colIndex).ToString();
                                    int numCols = toColumnId - fromColumnId;
                                    twoCellAnchor.ToMarker.ColumnId.Text = (colIndex + numCols).ToString();
                                }

                                // Update row position
                                uint fromRowId, toRowId;
                                if (uint.TryParse(twoCellAnchor.FromMarker.RowId.Text, out fromRowId) &&
                                    uint.TryParse(twoCellAnchor.ToMarker.RowId.Text, out toRowId))
                                {
                                    twoCellAnchor.FromMarker.RowId.Text = (rowIndex).ToString();
                                    uint numRows = toRowId - fromRowId;
                                    twoCellAnchor.ToMarker.RowId.Text = (rowIndex + numRows).ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the position of the anchor element (button location) within the Worksheet AlternateContent element.
        /// The following is a snippet of XML from /xl/worksheets/sheet.xml:
        ///      <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        ///      <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" xmlns:x14="http://schemas.microsoft.com/office/spreadsheetml/2009/9/main" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" mc:Ignorable="x14ac" xmlns:x14ac="http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac">
        ///        <sheetPr codeName="Sheet14"/>
        ///        <dimension ref="A1:AA124"/>
        ///        <sheetViews> ... </sheetViews>
        ///        <cols> ... </cols>
        ///        <sheetData> ... </sheetData>
        ///        ...
        ///        <mc:AlternateContent xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
        ///          <mc:Choice Requires="x14">
        ///            <controls>
        ///              <mc:AlternateContent xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
        ///                <mc:Choice Requires="x14">
        ///                  <control shapeId="2" r:id="rId4" name="Button 3">
        ///                    <controlPr defaultSize="0" print="0" autoFill="0" autoPict="0" macro="[0]!AddTaskAndResourceTables">
        ///                      <anchor moveWithCells="1">
        ///                        <from>
        ///                          <xdr:col>1</xdr:col>
        ///                          <xdr:colOff>1628775</xdr:colOff>
        ///                          <xdr:row>2</xdr:row>
        ///                          <xdr:rowOff>76200</xdr:rowOff>
        ///                        </from>
        ///                        <to>
        ///                          <xdr:col>2</xdr:col>
        ///                          <xdr:colOff>628650</xdr:colOff>
        ///                          <xdr:row>3</xdr:row>
        ///                          <xdr:rowOff>123825</xdr:rowOff>
        ///                        </to>
        ///                      </anchor>
        ///                    </controlPr>
        ///                  </control>
        ///                </mc:Choice>
        ///              </mc:AlternateContent>
        ///              <mc:AlternateContent xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
        ///                <mc:Choice Requires="x14">
        ///                  <control shapeId="3" r:id="rId5" name="Button 2">
        ///                    <controlPr defaultSize="0" print="0" autoFill="0" autoPict="0" macro="[0]!AddNewResourceRow">
        ///                      <anchor moveWithCells="1">
        ///                        <from>
        ///                          <xdr:col>1</xdr:col>
        ///                          <xdr:colOff>152400</xdr:colOff>
        ///                          <xdr:row>30</xdr:row>
        ///                          <xdr:rowOff>38100</xdr:rowOff>
        ///                        </from>
        ///                        <to>
        ///                          <xdr:col>1</xdr:col>
        ///                          <xdr:colOff>1276350</xdr:colOff>
        ///                          <xdr:row>31</xdr:row>
        ///                          <xdr:rowOff>85725</xdr:rowOff>
        ///                        </to>
        ///                      </anchor>
        ///                    </controlPr>
        ///                  </control>
        ///                </mc:Choice>
        ///              </mc:AlternateContent>
        ///            </controls>
        ///          </mc:Choice>
        ///        </mc:AlternateContent>
        ///        <tableParts count="6"> ... </tableParts>
        ///      </worksheet>
        ///      
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToUpdateId"></param>
        /// <param name="colIndexAdjustment"></param>
        /// <param name="rowIndex"></param>
        private static void UpdateWsAlternateContent(WorksheetPart worksheetPart, uint buttonToUpdateId, int colIndex, uint rowIndex)
        {
            //Update AlternateContent in Worksheet.xml
            foreach (AlternateContent wsAlternateContent in worksheetPart.Worksheet.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice wsAlternateContentChoice in wsAlternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (Controls wsControl in wsAlternateContentChoice.Elements<Controls>())
                    {
                        foreach (AlternateContent alternateContent in wsControl.Elements<AlternateContent>())
                        {
                            foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                            {
                                foreach (Control control in alternateContentChoice.Elements<Control>())
                                {
                                    // Is this the shape we want to update?
                                    string shapeId = control.ShapeId;
                                    if (shapeId.Equals(buttonToUpdateId.ToString()))
                                    {
                                        foreach (ObjectAnchor anchor in control.ControlProperties.Elements<ObjectAnchor>())
                                        {
                                            // Update column position
                                            int fromColumnId, toColumnId;
                                            if (int.TryParse(anchor.FromMarker.ColumnId.Text, out fromColumnId) &&
                                                int.TryParse(anchor.ToMarker.ColumnId.Text, out toColumnId))
                                            {
                                                anchor.FromMarker.ColumnId.Text = (colIndex).ToString();
                                                int numCols = toColumnId - fromColumnId;
                                                anchor.ToMarker.ColumnId.Text = (colIndex + numCols).ToString();
                                            }

                                            // Update row position
                                            uint fromRowId, toRowId;
                                            if (uint.TryParse(anchor.FromMarker.RowId.Text, out fromRowId) &&
                                                uint.TryParse(anchor.ToMarker.RowId.Text, out toRowId))
                                            {
                                                anchor.FromMarker.RowId.Text = (rowIndex).ToString();
                                                uint numRows = toRowId - fromRowId;
                                                anchor.ToMarker.RowId.Text = (rowIndex + numRows).ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clone the specified button and set the cloned button location.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToCloneId"></param>
        /// <param name="toColIndex"></param>
        /// <param name="toRowIndex"></param>
        private static void CloneButton(WorksheetPart worksheetPart, uint buttonToCloneId, int toColIndex, uint toRowIndex)
        {
            // Loop through the current set of shapes (e.g. buttons) to find the one with the highest shapeID.
            uint maxShapeId = 0;        // max shape Id
            foreach (AlternateContent wsDrAlternateContent in worksheetPart.DrawingsPart.WorksheetDrawing.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice alternateContentChoice in wsDrAlternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (TwoCellAnchor twoCellAnchor in alternateContentChoice.Elements<TwoCellAnchor>())
                    {
                        foreach (Shape shape in twoCellAnchor.Elements<Shape>())
                        {
                            // Keep track of the shape object with the highest ID
                            if (shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id > maxShapeId)
                            {
                                maxShapeId = shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id;
                            }
                        }
                    }
                }
            }

            // When updating or copying a button, we need to work with 3 XML components:
            //  Component                               Excel package location
            //  --------------------------------------  ---------------------------------------------------
            //  1) VmlDrawing,                          /xl/drawings/vmldrawing1.vml, vmldrawing2.vml, etc.
            //  2) worksheet drawing AlternateContent,  /xl/drawings/drawing1.xml, drawing2.xml, etc.
            //  3) worksheet AlternateContent           /xl/worksheets/sheet1.xml, sheet2.xml, etc.
            if (buttonToCloneId > 0)
            {
                // Clone a new button
                uint newButtonId = maxShapeId + 1;
                CloneVmlDrawing(worksheetPart, buttonToCloneId, newButtonId, toColIndex, toRowIndex);
                CloneWsDrAlternateContent(worksheetPart, buttonToCloneId, newButtonId, toColIndex, toRowIndex);
                CloneWsAlternateContent(worksheetPart, buttonToCloneId, newButtonId, toColIndex, toRowIndex);
            }
        }

        /// <summary>
        /// Clone the specified shape (button) within the VmlDrawing element.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToCloneId"></param>
        /// <param name="newButtonId"></param>
        /// <param name="colIndexAdjustment"></param>
        /// <param name="rowIndex"></param>
        private static void CloneVmlDrawing(WorksheetPart worksheetPart, uint buttonToCloneId, uint newButtonId, int colIndex, uint rowIndex)
        {
            // Copy VmlDrawingParts
            foreach (VmlDrawingPart vmlDrawingPart in worksheetPart.VmlDrawingParts)
            {
                //Parse the vmlDrawingPart XML
                XmlDocument xmldoc = new XmlDocument();
                using (Stream stream = vmlDrawingPart.GetStream())
                {
                    xmldoc.Load(stream);
                }

                //Instantiate an XmlNamespaceManager object. 
                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmldoc.NameTable);

                //Add the namespaces used in vmlDrawingPart to the XmlNamespaceManager.
                xmlnsManager.AddNamespace("v", "urn:schemas-microsoft-com:vml");
                xmlnsManager.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
                xmlnsManager.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");

                //Search for shape node with matching ID
                string shapeId = ImportExportConstants.SHAPE_ID_PREFIX + buttonToCloneId;
                XmlNodeList shapeNodes = xmldoc.SelectNodes("//v:shape[@id='" + shapeId + "']", xmlnsManager);
                if (shapeNodes != null && shapeNodes.Count > 0)
                {
                    // make a copy of the shapeNode
                    XmlNode newShapeNode = shapeNodes[0].CloneNode(true);
                    // give it a unique ID
                    string ns = xmldoc.FirstChild.GetNamespaceOfPrefix("v");
                    XmlNode idAttr = xmldoc.CreateNode(XmlNodeType.Attribute, "id", ns);
                    idAttr.Value = ImportExportConstants.SHAPE_ID_PREFIX + newButtonId;
                    newShapeNode.Attributes.RemoveNamedItem("id");  // remove old ID attribute
                    newShapeNode.Attributes.SetNamedItem(idAttr);   // add new ID attribute

                    XmlNode clientData = newShapeNode.SelectSingleNode("x:ClientData", xmlnsManager);
                    XmlNode anchor = clientData.SelectSingleNode("x:Anchor", xmlnsManager);
                    foreach (XmlNode anchorText in anchor.ChildNodes)
                    {
                        // Process the anchor element values as follows:
                        //  <x:Anchor>fromColId, fromColOffset, fromRowId, fromRowOffset, toColId, toColOffset, toRowId, toRowOffset</x:Anchor>
                        // For example:
                        //  <x:Anchor>1, 171, 2, 8, 2, 66, 3, 13</x:Anchor>
                        string value = anchorText.Value.Trim(); // remove any whitespace, e.g. \r\n
                        string[] tokens = value.Split(',');
                        int fromColId, toColId;
                        uint fromRowId, toRowId;
                        if (int.TryParse(tokens[0], out fromColId) &&
                            uint.TryParse(tokens[2], out fromRowId) &&
                            int.TryParse(tokens[4], out toColId) &&
                            uint.TryParse(tokens[6], out toRowId))
                        {
                            // Adjust the row/column Ids (leave row/column offsets unchanged)
                            int numCols = toColId - fromColId;
                            uint numRows = toRowId - fromRowId;
                            tokens[0] = (colIndex).ToString();
                            tokens[2] = (rowIndex).ToString();
                            tokens[4] = (colIndex + numCols).ToString();
                            tokens[6] = (rowIndex + numRows).ToString();

                            value = string.Join(",", tokens);
                            anchorText.Value = value;
                        }
                    }
                    xmldoc.DocumentElement.AppendChild(newShapeNode);
                }

                //Store the updated vmlDrawingPart
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmldoc.Save(memoryStream);
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    StreamReader sr = new StreamReader(memoryStream);
                    string vmlDrawingPartString = sr.ReadToEnd();
                    GenerateVmlDrawingPartContent(vmlDrawingPart, vmlDrawingPartString);
                }
            }
        }

        /// <summary>
        /// Clone the specified AlternateContent element (button) within the Worksheet Drawing AlternateContent.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToCloneId"></param>
        /// <param name="newButtonId"></param>
        /// <param name="colIndexAdjustment"></param>
        /// <param name="rowIndex"></param>
        private static void CloneWsDrAlternateContent(WorksheetPart worksheetPart, uint buttonToCloneId, uint newButtonId, int colIndex, uint rowIndex)
        {
            string newShapeId = ImportExportConstants.SHAPE_ID_PREFIX + newButtonId;
            string newButtonName = ImportExportConstants.BUTTON_PREFIX + newButtonId;

            foreach (AlternateContent alternateContent in worksheetPart.DrawingsPart.WorksheetDrawing.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (TwoCellAnchor twoCellAnchor in alternateContentChoice.Elements<TwoCellAnchor>())
                    {
                        foreach (Shape shape in twoCellAnchor.Elements<Shape>())
                        {
                            // Is this the anchor/shape we want to copy?
                            if (shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id == buttonToCloneId)
                            {
                                // Clone AlternateContent element
                                AlternateContent newAlternateContent = (AlternateContent)alternateContent.CloneNode(true);
                                // Update Id, Name, and ShapeId with unique values
                                foreach (AlternateContentChoice newAlternateContentChoice in newAlternateContent.Elements<AlternateContentChoice>())
                                {
                                    foreach (TwoCellAnchor newTwoCellAnchor in newAlternateContentChoice.Elements<TwoCellAnchor>())
                                    {
                                        foreach (Shape newShape in newTwoCellAnchor.Elements<Shape>())
                                        {
                                            newShape.NonVisualShapeProperties.NonVisualDrawingProperties.Id = newButtonId;
                                            newShape.NonVisualShapeProperties.NonVisualDrawingProperties.Name = newButtonName;

                                            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                            // Note: The code used to update the shapeId property depends on the version
                                            //       of DocumentFormat.OpenXml dll.
                                            //
                                            // The following code will be compatible with 
                                            //  Open XML Format SDK 2.5 - DocumentFormat.OpenXml V2.5.5631.0 or higher (when available)
                                            // 
                                            //  foreach (var nonVisualDrawingPropertiesExtension in newShape.NonVisualShapeProperties.NonVisualDrawingProperties.NonVisualDrawingPropertiesExtensionList.Elements<DocumentFormat.OpenXml.Drawing.NonVisualDrawingPropertiesExtension>())
                                            //  {
                                            //      foreach (var compatExtension in nonVisualDrawingPropertiesExtension.Elements())
                                            //      {
                                            //         ((A14.CompatExtension)compatExtension).ShapeId = newShapeId;
                                            //      }
                                            //  }
                                            //
                                            // The following code is compatible with 
                                            //  Open XML Format SDK 2.0 - DocumentFormat.OpenXml V2.0.5022.0
                                            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                            foreach (A.Extension nonVisualDrawingPropertiesExtension in newShape.NonVisualShapeProperties.NonVisualDrawingProperties.ExtensionList.Elements<A.Extension>())
                                            {
                                                int i = 0;
                                                foreach (OpenXmlElement compatExtension in nonVisualDrawingPropertiesExtension.Elements())
                                                {
                                                    string outerxml = nonVisualDrawingPropertiesExtension.OuterXml;
                                                    foreach (OpenXmlAttribute attr in compatExtension.ExtendedAttributes)
                                                    {
                                                        string localName = attr.LocalName;
                                                        if (localName.Equals(ImportExportConstants.SPID_ATTRIBUTE))
                                                        {
                                                            string oldShapeId = attr.Value;
                                                            string newXml = outerxml.Replace(oldShapeId, newShapeId);
                                                            newShape.NonVisualShapeProperties.NonVisualDrawingProperties.ExtensionList.ReplaceChild(new A.Extension(newXml), nonVisualDrawingPropertiesExtension);
                                                        }
                                                    }
                                                    i++;
                                                }
                                            }
                                            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                        }

                                        // Update column position
                                        int fromColumnId, toColumnId;
                                        if (int.TryParse(newTwoCellAnchor.FromMarker.ColumnId.Text, out fromColumnId) &&
                                            int.TryParse(newTwoCellAnchor.ToMarker.ColumnId.Text, out toColumnId))
                                        {
                                            newTwoCellAnchor.FromMarker.ColumnId.Text = (colIndex).ToString();
                                            int numCols = toColumnId - fromColumnId;
                                            newTwoCellAnchor.ToMarker.ColumnId.Text = (colIndex + numCols).ToString();
                                        }

                                        // Update row position
                                        uint fromRowId, toRowId;
                                        if (uint.TryParse(newTwoCellAnchor.FromMarker.RowId.Text, out fromRowId) &&
                                            uint.TryParse(newTwoCellAnchor.ToMarker.RowId.Text, out toRowId))
                                        {
                                            newTwoCellAnchor.FromMarker.RowId.Text = (rowIndex).ToString();
                                            uint numRows = toRowId - fromRowId;
                                            newTwoCellAnchor.ToMarker.RowId.Text = (rowIndex + numRows).ToString();
                                        }
                                    }
                                }
                                worksheetPart.DrawingsPart.WorksheetDrawing.Append(newAlternateContent);
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clone the specified AlternateContent element (button) within the Worksheet AlternateContent.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonToCloneId"></param>
        /// <param name="newButtonId"></param>
        /// <param name="colIndexAdjustment"></param>
        /// <param name="rowIndex"></param>
        private static void CloneWsAlternateContent(WorksheetPart worksheetPart, uint buttonToCloneId, uint newButtonId, int colIndex, uint rowIndex)
        {
            string newButtonName = ImportExportConstants.BUTTON_PREFIX + newButtonId;

            foreach (AlternateContent wsAlternateContent in worksheetPart.Worksheet.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice wsAlternateContentChoice in wsAlternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (Controls wsControl in wsAlternateContentChoice.Elements<Controls>())
                    {
                        foreach (AlternateContent alternateContent in wsControl.Elements<AlternateContent>())
                        {
                            foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                            {
                                foreach (Control control in alternateContentChoice.Elements<Control>())
                                {
                                    // Is this the anchor/shape we want to copy?
                                    string shapeId = control.ShapeId;
                                    if (shapeId.Equals(buttonToCloneId.ToString()))
                                    {
                                        // Clone AlternateContent element
                                        AlternateContent newAlternateContent = (AlternateContent)alternateContent.CloneNode(true);
                                        // Update ShapeId, and Name with unique values
                                        foreach (AlternateContentChoice newAlternateContentChoice in newAlternateContent.Elements<AlternateContentChoice>())
                                        {
                                            foreach (Control newControl in newAlternateContentChoice.Elements<Control>())
                                            {
                                                newControl.ShapeId = newButtonId;
                                                newControl.Name = newButtonName;
                                                foreach (ObjectAnchor newAnchor in newControl.ControlProperties.Elements<ObjectAnchor>())
                                                {
                                                    // Update column position
                                                    int fromColumnId, toColumnId;
                                                    if (int.TryParse(newAnchor.FromMarker.ColumnId.Text, out fromColumnId) &&
                                                        int.TryParse(newAnchor.ToMarker.ColumnId.Text, out toColumnId))
                                                    {
                                                        newAnchor.FromMarker.ColumnId.Text = (colIndex).ToString();
                                                        int numCols = toColumnId - fromColumnId;
                                                        newAnchor.ToMarker.ColumnId.Text = (colIndex + numCols).ToString();
                                                    }

                                                    // Update row position
                                                    uint fromRowId, toRowId;
                                                    if (uint.TryParse(newAnchor.FromMarker.RowId.Text, out fromRowId) &&
                                                        uint.TryParse(newAnchor.ToMarker.RowId.Text, out toRowId))
                                                    {
                                                        newAnchor.FromMarker.RowId.Text = (rowIndex).ToString();
                                                        uint numRows = toRowId - fromRowId;
                                                        newAnchor.ToMarker.RowId.Text = (rowIndex + numRows).ToString();
                                                    }
                                                }
                                            }
                                        }
                                        wsControl.Append(newAlternateContent);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stream out VmlDrawingPart contents.
        /// </summary>
        /// <param name="vmlDrawingPart"></param>
        /// <param name="vmlDrawingPartString"></param>
        private static void GenerateVmlDrawingPartContent(VmlDrawingPart vmlDrawingPart, string vmlDrawingPartString)
        {
            XmlTextWriter writer = new XmlTextWriter(vmlDrawingPart.GetStream(FileMode.Create), System.Text.Encoding.UTF8);
            writer.WriteRaw(vmlDrawingPartString);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Remove specified button from worksheet.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="buttonId"></param>
        private static void RemoveButton(WorksheetPart worksheetPart, uint buttonToRemoveId)
        {
            // When removing a button, we need to work with 3 XML components:
            //  Component                               Excel package location
            //  --------------------------------------  ---------------------------------------------------
            //  1) VmlDrawing,                          /xl/drawings/vmldrawing1.vml, vmldrawing2.vml, etc.
            //  2) worksheet drawing AlternateContent,  /xl/drawings/drawing1.xml, drawing2.xml, etc.
            //  3) worksheet AlternateContent           /xl/worksheets/sheet1.xml, sheet2.xml, etc.
            //
            RemoveVmlDrawing(worksheetPart, buttonToRemoveId);
            RemoveWsDrAlternateContent(worksheetPart, buttonToRemoveId);
            RemoveWsAlternateContent(worksheetPart, buttonToRemoveId);
        }

        /// <summary>
        ///  Remove the specified shape (button) from the VmlDrawing element.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="butonToRemoveId"></param>
        private static void RemoveVmlDrawing(WorksheetPart worksheetPart, uint butonToRemoveId)
        {
            foreach (VmlDrawingPart vmlDrawingPart in worksheetPart.VmlDrawingParts)
            {
                //Parse the vmlDrawingPart XML
                XmlDocument xmldoc = new XmlDocument();
                using (Stream stream = vmlDrawingPart.GetStream())
                {
                    xmldoc.Load(stream);
                }

                //Instantiate an XmlNamespaceManager object. 
                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmldoc.NameTable);

                //Add the namespaces used in vmlDrawingPart to the XmlNamespaceManager.
                xmlnsManager.AddNamespace("v", "urn:schemas-microsoft-com:vml");
                xmlnsManager.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
                xmlnsManager.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");

                //Search for shape node with matching ID
                string shapeId = "_x0000_s" + butonToRemoveId;
                XmlNodeList shapeNodes = xmldoc.SelectNodes("//v:shape[@id='" + shapeId + "']", xmlnsManager);
                foreach (XmlNode shapeNode in shapeNodes)
                {
                    shapeNode.ParentNode.RemoveChild(shapeNode);
                }

                //Store the updated vmlDrawingPart
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmldoc.Save(memoryStream);
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    StreamReader sr = new StreamReader(memoryStream);
                    string vmlDrawingPartString = sr.ReadToEnd();
                    GenerateVmlDrawingPartContent(vmlDrawingPart, vmlDrawingPartString);
                }
            }
        }

        /// <summary>
        /// Remove the specified AlternateContent element (button) from the Worksheet Drawing AlternateContent.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="butonToRemoveId"></param>
        private static void RemoveWsDrAlternateContent(WorksheetPart worksheetPart, uint butonToRemoveId)
        {
            foreach (AlternateContent alternateContent in worksheetPart.DrawingsPart.WorksheetDrawing.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (TwoCellAnchor twoCellAnchor in alternateContentChoice.Elements<TwoCellAnchor>())
                    {
                        // Is this the AlternateContent we want to remove?
                        foreach (Shape shape in twoCellAnchor.Elements<Shape>())
                        {
                            if (shape.NonVisualShapeProperties.NonVisualDrawingProperties.Id == butonToRemoveId)
                            {
                                alternateContent.Remove();
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove the specified AlternateContent element (button) from the Worksheet AlternateContent.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="butonToRemoveId"></param>
        private static void RemoveWsAlternateContent(WorksheetPart worksheetPart, uint butonToRemoveId)
        {
            foreach (AlternateContent wsAlternateContent in worksheetPart.Worksheet.Elements<AlternateContent>())
            {
                foreach (AlternateContentChoice wsAlternateContentChoice in wsAlternateContent.Elements<AlternateContentChoice>())
                {
                    foreach (Controls wsControl in wsAlternateContentChoice.Elements<Controls>())
                    {
                        foreach (AlternateContent alternateContent in wsControl.Elements<AlternateContent>())
                        {
                            foreach (AlternateContentChoice alternateContentChoice in alternateContent.Elements<AlternateContentChoice>())
                            {
                                foreach (Control control in alternateContentChoice.Elements<Control>())
                                {
                                    // Is this the AlternateContent we want to remove?
                                    string shapeId = control.ShapeId;
                                    if (shapeId.Equals(butonToRemoveId.ToString()))
                                    {
                                        alternateContent.Remove();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Used to reference Workspace, BOE, Task and Resource Table Templates from template worksheet.
    /// </summary>
    public class WorkofflineTemplate
    {
        /// <summary>
        /// Constructor for initializing object based on template worksheet.
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="templateSheetName"></param>
        public WorkofflineTemplate(SpreadsheetDocument spreadsheet, string templateSheetName)
        {
            if (spreadsheet == null) { throw new ArgumentNullException(nameof(spreadsheet)); }
            if (templateSheetName == null) { throw new ArgumentNullException(nameof(templateSheetName)); }

            // Get a collection of shared strings in the document for pulling out cell values
            SharedStringTablePart sharedStringPart = spreadsheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            this.SharedStringItems = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
            this.Stylesheet = spreadsheet.WorkbookPart.WorkbookStylesPart.Stylesheet;

            // Get WorksheetPart for template worksheet.
            this.TemplateSheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, templateSheetName);

            // Get TableRange for Workspace, BOE, Task and Resource Tables and corresponding Rows from template
            Collection<Table> tables = ExcelUtilities.GetTablesInWorksheetPart(this.TemplateSheetPart);
            foreach (Table table in tables)
            {
                if (table.Name.HasValue)
                {
                    switch (table.Name.Value)
                    {
                        case ImportExportConstants.WORKSPACE_TABLE_TEMPLATE:
                            this.WorkspaceTable = new TableRange(templateSheetName, table);
                            this.WorkspaceTableRows = this.GetTableRows(this.TemplateSheetPart, this.WorkspaceTable);
                            break;
                        case ImportExportConstants.BOE_TABLE_TEMPLATE:
                            this.BoeTable = new TableRange(templateSheetName, table);
                            this.BoeTableRows = this.GetTableRows(this.TemplateSheetPart, this.BoeTable);
                            // Get styles to be used when populating BOE custom fields.
                            // Use the styles from the last BOE table row.
                            IList<Cell> boeLastRowCells = this.BoeTableRows[ImportExportConstants.BOE_FIRST_CUSTOMFIELD_ROW_OFFSET - 1].Elements<Cell>().ToList<Cell>();
                            if (boeLastRowCells.Count > 1)
                            {
                                this.BoeCustomFieldNameStyleIndex = boeLastRowCells[1].StyleIndex;
                            }
                            if (boeLastRowCells.Count > 2)
                            {
                                this.BoeCustomFieldValueStyleIndex = boeLastRowCells[2].StyleIndex;
                            }
                            break;
                        case ImportExportConstants.TASK_TABLE_TEMPLATE:
                            this.TaskTable = new TableRange(templateSheetName, table);
                            this.TaskTableRows = this.GetTableRows(this.TemplateSheetPart, this.TaskTable);
                            // Get styles to be used when populating Task custom fields.
                            // Use the styles from the last Task table row.
                            IList<Cell> taskLastRowCells = this.TaskTableRows[ImportExportConstants.TASK_FIRST_CUSTOMFIELD_ROW_OFFSET - 1].Elements<Cell>().ToList<Cell>();
                            if (taskLastRowCells.Count > 1)
                            {
                                this.TaskCustomFieldNameStyleIndex = taskLastRowCells[1].StyleIndex;
                            }
                            if (taskLastRowCells.Count > 2)
                            {
                                this.TaskCustomFieldValueStyleIndex = taskLastRowCells[2].StyleIndex;
                            }
                            break;
                        case ImportExportConstants.RESOURCE_TABLE_TEMPLATE:
                            this.ResourceTable = new TableRange(templateSheetName, table);
                            this.ResourceTableRows = this.GetTableRows(this.TemplateSheetPart, this.ResourceTable);
                            break;
                        case ImportExportConstants.DYNAMIC_COLUMNS_TEMPLATE:
                            // Get cell styles for dynamically generated resource columns (custom fields, and monthly spread hours and cost)
                            this.DynamicColumnsTable = new TableRange(templateSheetName, table);
                            this.DynamicColumnsRows = this.GetTableRows(this.TemplateSheetPart, this.DynamicColumnsTable);
                            IList<Cell> bannerCells = this.DynamicColumnsRows[0].Elements<Cell>().ToList<Cell>();
                            IList<Cell> headerCells = this.DynamicColumnsRows[1].Elements<Cell>().ToList<Cell>();
                            IList<Cell> dataCells = this.DynamicColumnsRows[2].Elements<Cell>().ToList<Cell>();
                            this.ResourceBannerCellStyle = bannerCells[0].StyleIndex;
                            this.ResourceCustomHeaderStyleIndex = headerCells[0].StyleIndex;
                            this.ResourceHoursHeaderStyleIndex = headerCells[1].StyleIndex;
                            this.ResourceCostHeaderStyleIndex = headerCells[2].StyleIndex;
                            this.ResourceCustomStyleIndex = dataCells[0].StyleIndex;
                            this.ResourceHoursStyleIndex = dataCells[1].StyleIndex;
                            this.ResourceCostStyleIndex = dataCells[2].StyleIndex;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the set of data rows within the specified table.
        /// Only returns rows/cells within the table boundaries.
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="tableRange"></param>
        /// <returns></returns>
        private List<Row> GetTableRows(WorksheetPart worksheetPart, TableRange tableRange)
        {
            List<Row> rows = new List<Row>();

            // Copy rows from the template table into the target worksheet (including any styles or formatting).
            foreach (Row templateRow in worksheetPart.Worksheet.Descendants<Row>()
                                                               .Where(c => c.RowIndex >= tableRange.RowBegin &&
                                                                           c.RowIndex <= tableRange.RowEnd))
            {
                // Create new row based on template row
                Row row = new Row(templateRow.OuterXml);

                // Only keep cells within the template table range
                long tableColBegin = TableRange.GetColumnNumber(tableRange.ColumnBegin);
                long tableColEnd = TableRange.GetColumnNumber(tableRange.ColumnEnd);
                IList<Cell> rowCells = row.Elements<Cell>().ToList();
                row.RemoveAllChildren<Cell>();
                foreach (Cell cell in rowCells)
                {
                    long colIndex = TableRange.GetColumnNumber(ExcelUtilities.ParseColumnName(cell.CellReference.Value));
                    if (colIndex >= tableColBegin && colIndex <= tableColEnd)
                    {
                        row.AppendChild<Cell>(cell);
                    }
                }

                rows.Add(row);
            }
            return rows;
        }

        /// <summary>
        /// Generate Table Definition Part from template.
        /// </summary>
        /// <param name="templateTableRange"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="newTableDefPartId"></param>
        /// <param name="newTableName"></param>
        /// <param name="newTableReference"></param>
        /// <returns></returns>
        public static TableDefinitionPart GenerateTableDefinitionPartFromTemplate(
            TableRange templateTableRange, OpenXmlPartContainer worksheetPart, uint newTableDefPartId, string newTableName,
            long newColBeginIndex, uint newRowBeginIndex, long newColEndIndex, uint newRowEndIndex)
        {
            // Check inputs
            if (templateTableRange == null) { throw new ArgumentNullException(nameof(templateTableRange)); }
            if (worksheetPart == null) { throw new ArgumentNullException(nameof(worksheetPart)); }
            if (newTableName == null) { throw new ArgumentNullException(nameof(newTableName)); }

            // Generate new Table Range Reference, e.g. "A3:P25"
            string newTableReference = TableRange.GetColumnName(newColBeginIndex) + newRowBeginIndex + ":" + TableRange.GetColumnName(newColEndIndex) + newRowEndIndex;

            // Create a new TableDefinitionPart
            TableDefinitionPart newTableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
            // Create a new Table based off the template
            Table table = new Table(templateTableRange.Table.OuterXml)
            {
                Id = newTableDefPartId,
                Name = newTableName,
                DisplayName = newTableName,
                Reference = newTableReference
            };

            // Get target cell range for new table definition and generate TableColumns accordingly.
            uint columnId = Convert.ToUInt32(newColBeginIndex);
            table.TableColumns = new TableColumns();
            foreach (TableColumn templateColumn in templateTableRange.Table.TableColumns)
            {
                columnId++;
                TableColumn tableColumn = new TableColumn(templateColumn.OuterXml);
                tableColumn.Id = columnId;
                tableColumn.Name = ImportExportConstants.COLUMN_PREFIX + columnId;
                table.TableColumns.AppendChild<TableColumn>(tableColumn);
            }

            // Add additional TableColumns (if necessary)
            long additionalColumns = (newColEndIndex - newColBeginIndex) - (templateTableRange.ColumnEndIndex - templateTableRange.ColumnBeginIndex);
            for (long i = 0; i < additionalColumns; i++)
            {
                columnId++;
                table.TableColumns.AppendChild<TableColumn>(new TableColumn()
                {
                    Id = columnId,
                    Name = ImportExportConstants.COLUMN_PREFIX + columnId
                });
            }

            table.TableColumns.Count = Convert.ToUInt32(table.TableColumns.ChildElements.Count);
            table.Reference = newTableReference;
            newTableDefinitionPart.Table = table;
            return newTableDefinitionPart;
        }

        /// <summary>
        /// Copy the contents of a template table to the specified colIndex/rowIndex in the target worksheet.
        /// </summary>
        /// <param name="templateReference"></param>
        /// <param name="templateRows"></param>
        /// <param name="worksheetPart"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        public static uint CopyTemplateTable(TableRange templateTableRange, IList<Row> templateRows, WorksheetPart worksheetPart, long colIndex, uint rowIndex)
        {
            // Check inputs
            if (templateTableRange == null) { throw new ArgumentNullException(nameof(templateTableRange)); }
            if (templateRows == null) { throw new ArgumentNullException(nameof(templateRows)); }
            if (worksheetPart == null) { throw new ArgumentNullException(nameof(worksheetPart)); }

            // Calculate column offset for moving template table to target worksheet
            long colOffset = colIndex - templateTableRange.ColumnBeginIndex;

            // Copy rows from the template table to the target worksheet location (including any styles or formatting).
            foreach (Row templateRow in templateRows)
            {
                // Create new row based on template row
                Row row = new Row(templateRow.OuterXml)
                {
                    RowIndex = rowIndex
                };

                // Reset cell references according to new target location
                ICollection<Cell> cellsInRow = row.Elements<Cell>().ToList();
                if (cellsInRow.Any())
                {
                    foreach (Cell cell in cellsInRow)
                    {
                        // Update the references for the rows cells.
                        string oldReference = cell.CellReference.Value;
                        long newColIndex = TableRange.GetColumnNumber(ExcelUtilities.ParseColumnName(oldReference)) + colOffset;
                        cell.CellReference = TableRange.GetColumnName(newColIndex) + rowIndex;
                    }
                }

                ExcelUtilities.InsertRow(rowIndex, worksheetPart, row, true);

                if (rowIndex == uint.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(rowIndex), "rowIndex must be less than UInt32.MaxValue");
                }

                rowIndex++;
            }
            return rowIndex;
        }

        /// <summary>
        /// The "Add New Task" and "Add Resource" button macros use counters stored in the
        /// template tables to assign the next unique, negative ID.
        /// This method will find the Resource ID counter, decrement it, and return the current value. 
        /// </summary>
        /// <param name="counterName"></param>
        /// <returns></returns>
        public int DecrementResourceIdCounter()
        {
            int counterValue = -1;  // default value

            long columnIndex = this.ResourceTable.ColumnBeginIndex + ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_COLUMN_OFFSET;
            uint rowIndex = this.ResourceTable.RowBegin + ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET;
            Cell cell = ExcelUtilities.GetCell(this.TemplateSheetPart, TableRange.GetColumnName(columnIndex) + rowIndex);
            if (cell != null)
            {
                string value = ExcelUtilities.GetCellValue(cell, this.SharedStringItems, this.Stylesheet);
                if (int.TryParse(value, out counterValue))
                {
                    counterValue--;
                }
                else
                {
                    counterValue = -1;
                }
            }
            ExcelUtilities.SetCellValue(this.TemplateSheetPart.Worksheet, rowIndex, columnIndex, CellValues.Number, counterValue.ToString(), null, true);
            return counterValue;
        }


        private SharedStringItem[] SharedStringItems { get; set; }
        private Stylesheet Stylesheet { get; set; }
        public WorksheetPart TemplateSheetPart { get; set; }

        // Template tables and associated Row data
        public TableRange WorkspaceTable { get; set; }
        public IList<Row> WorkspaceTableRows { get; set; }
        public TableRange BoeTable { get; set; }
        public IList<Row> BoeTableRows { get; set; }
        public TableRange TaskTable { get; set; }
        public IList<Row> TaskTableRows { get; set; }
        public TableRange ResourceTable { get; set; }
        public IList<Row> ResourceTableRows { get; set; }
        public TableRange DynamicColumnsTable { get; set; }
        public IList<Row> DynamicColumnsRows { get; set; }

        // Style Index values for dynamically generated cells
        public uint BoeCustomFieldNameStyleIndex { get; set; }
        public uint BoeCustomFieldValueStyleIndex { get; set; }
        public uint TaskCustomFieldNameStyleIndex { get; set; }
        public uint TaskCustomFieldValueStyleIndex { get; set; }
        public uint ResourceBannerCellStyle { get; set; }
        public uint ResourceCustomHeaderStyleIndex { get; set; }
        public uint ResourceCustomStyleIndex { get; set; }
        public uint ResourceHoursHeaderStyleIndex { get; set; }
        public uint ResourceHoursStyleIndex { get; set; }
        public uint ResourceCostHeaderStyleIndex { get; set; }
        public uint ResourceCostStyleIndex { get; set; }
    }

}