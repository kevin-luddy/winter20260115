// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.MOQ;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    public class WorkOfflineImporter : IWorkOfflineImporter
    {
        private IPermissionsDTODataLoader _IPermissionsDTOLoader;
        private ICommonDataMapper _CommonDataMapper;
        private IResourceDTODataLoader resourceDataLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private IVariableCircularReferenceChecker _VariableCircularReferenceChecker;
        
        string[] ExcludedSheetNames = { "Options Lists", "TableTemplates", "TestTemplates" };
        
        public WorkOfflineImporter(
            IPermissionsDTODataLoader inIPermissionsDTOLoader,
            ICommonDataMapper inCommonDataMapper,
            IResourceDTODataLoader inResourceDataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IVariableCircularReferenceChecker inVariableCircularReferenceChecker)
        {
            this._IPermissionsDTOLoader = inIPermissionsDTOLoader;
            this._CommonDataMapper = inCommonDataMapper;
            this.resourceDataLoader = inResourceDataLoader;
            this.perfOrgLoader = perfOrgLoader;
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
        }

        /// <summary>
        /// Imports from excel file
        /// </summary>
        /// <param name="excelFileStream">excel file stream</param>
        /// <param name="ws">ws for which the import is happening</param>
        /// <returns>imported data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.ObjectModel.Collection`1<IES.Common.IO.Import.ImportedBoe>")]
        public WorkofflineImport ImportFromExcelFile(Stream excelFileStream, FullWorkspace ws)
        {
            if (excelFileStream == null) { throw new ArgumentNullException(nameof(excelFileStream)); }
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }

            ws.LoadBoesRTEData();
            ws.LoadMaterialsRTEData();
            ws.LoadODCsRTEData();
            ws.LoadTaskElementRTEData();
            ws.LoadTravelRTEData();

            WorkofflineImport importedResults = new WorkofflineImport();
            try
            {
                importedResults.ImportedBoes = new Collection<WorkofflineImportedBoe>();
                importedResults.ImportTypes = new Collection<WorkofflineImportResult>();

                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    MiscSpreadsheetElements elements = this.GetMiscSpreadsheetElements(document);

                    IList<int> BoeIDsProcessed = new List<int>();
                    // Get all boe tabs in the document
                    Sheets tabs = ExcelUtilities.GetWorksheetsFromDocument(document);
                    IList<Tuple<String, String>> sheetIDs = ExcelUtilities.GetWorksheetIDsFromSheets(tabs);
                    // Cycle through the tabs to retrieve the tables
                    foreach (Tuple<String, String> item in sheetIDs)
                    {
                        if (!this.ExcludedSheetNames.Contains(item.Item1))
                        {
                            WorksheetPart wsp = ExcelUtilities.GetWorksheetPartBySheetID(document.WorkbookPart, item.Item2);
                            IList<Table> worksheetTables = ExcelUtilities.GetTablesInWorksheetPart(wsp);

                            // Create tablerange objects for each table
                            IList<TableRange> tableRanges = new List<TableRange>();
                            foreach (Table table in worksheetTables)
                            {
                                tableRanges.Add(new TableRange(item.Item1, table));
                            }

                            // Get the Workspace table and TableRange
                            Table workspaceTable = worksheetTables.FirstOrDefault(x => x.Name.ToString().Contains(ImportExportConstants.WORKSPACE_TABLE_SUFFIX));
                            if (workspaceTable != null)
                            {
                                // get the TableRange for this table
                                TableRange importedWorkspaceTableRange = tableRanges.First(x => x.TableName == workspaceTable.Name.Value);
                                Collection<WorkofflineImportResult> results = this.ValidateWorkspaceInformation(ws, importedWorkspaceTableRange, wsp, elements);
                                foreach (WorkofflineImportResult result in results)
                                {
                                    importedResults.ImportTypes.Add(result);
                                }

                                if(results.Any())
                                    // if there are any errors don't continue
                                {
                                    throw new NotExcelFileException(String.Format("The Workspace ID was missing or invalid on the {0} tab", item.Item1));
                                }
                            }
                            else
                            {
                                // if there is no Workspace table there is a file error and there is no need to process any further
                                throw new NotExcelFileException(String.Format("The Workspace table was missing on the {0} tab", item.Item1));
                            }

                            // Get the Boe table and TableRange
                            Table BoeTable = worksheetTables.FirstOrDefault(x => x.Name.ToString().Contains(ImportExportConstants.BOE_TABLE_SUFFIX));
                            WorkofflineImportedBoe importedBoe = null;
                            if (BoeTable != null)
                            {
                                // get the TableRange for this table
                                TableRange importedBoeTableRange = tableRanges.First(x => x.TableName == BoeTable.Name.Value);
                                importedBoe = this.GetBoeDataFromTable(BoeIDsProcessed, ws, importedBoeTableRange, wsp, elements);

                                importedResults.ImportedBoes.Add(importedBoe);
                            }
                            else
                            {
                                // if there is no Boe table there is a file error and there is no need to process any further
                                throw new NotExcelFileException(String.Format("The BOE table was missing on {0} tab", item.Item1));
                            }

                            // if there is a problem with the boe id no need to process this tab any further
                            if (importedBoe.ImportTypes.Contains(BoeImportResult.BoeIDMissingInvalid) || 
                                importedBoe.ImportTypes.Contains(BoeImportResult.DuplicateBoeID) ||
                                importedBoe.ImportTypes.Contains(BoeImportResult.BoeDoesNotExist) ||
                                importedBoe.ImportTypes.Contains(BoeImportResult.BoeNotInDraftState))
                            {
                                continue;
                            }

                            IList<int> TaskIDsProcessed = new List<int>();

                            // Get the Task & Resource tables
                            ICollection<Table> ResourceTables = worksheetTables.Where(x => x.Name.ToString().Contains(ImportExportConstants.RESOURCE_TABLE_SUFFIX)).ToList();

                            FullBoe importedBoeObject = ws.Boes.FirstOrDefault(b => b.Id == importedBoe.Id);

                            foreach (Table table in worksheetTables.Where(x => x.Name.ToString().Contains(ImportExportConstants.TASK_TABLE_SUFFIX)))
                            {
                                String TableSequenceNumber = this.GetTableSequenceNumber(table.Name.ToString());

                                // get the TableRange for this table
                                TableRange importedTaskTableRange = tableRanges.First(x => x.TableName == table.Name.Value);
                                WorkofflineImportedTaskElement importedTask = this.GetTaskElementDataFromTable(TaskIDsProcessed, importedTaskTableRange, importedBoeObject, wsp, elements, ws);

                                importedTask.BoeID = importedBoe.Id;
                                TaskIDsProcessed.Add(importedTask.Id);
                                importedBoe.ImportedTaskElements.Add(importedTask);

                                // if there is a problem with the task id or the dates (if there is a problem with dates we can't verify the resource dates) no need to process this task or it's resources any further
                                if (importedTask.ImportTypes.Contains(TaskElementImportResult.TaskElementIDMissingOrInvalid) || importedTask.ImportTypes.Contains(TaskElementImportResult.StartDateMissingInvalid) ||
                                    importedTask.ImportTypes.Contains(TaskElementImportResult.EndDateMissingInvalid))
                                {
                                    continue;
                                }

                                BoeTaskElementDTO TaskElement = null;

                                if (importedTask.Id > 0)
                                {
                                    TaskElement = ws.TaskElements.FirstOrDefault(t => t.Id == importedTask.Id);
                                }

                                // get the Resource Table for this Task
                                Table ResourceTable = ResourceTables.FirstOrDefault(x => x.Name.ToString().Contains(ImportExportConstants.RESOURCE_TABLE_SUFFIX + TableSequenceNumber));
                                if (ResourceTable != null)
                                {
                                    IList<int> ResourceTypeIDsProcessed = new List<int>();
                                    // get the TableRange for this table
                                    TableRange importedResourceTableRange = tableRanges.First(x => x.TableName == ResourceTable.Name.Value);
                                    Cell ResourceTypeTableBegin = ExcelUtilities.GetCell(wsp, importedResourceTableRange.Begin);
                                    int SpreadStartColumn = ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET + this.FindSpreadStartColumnOffset(ResourceTypeTableBegin, wsp, elements);
                                    int CustomFieldsEndColumn = SpreadStartColumn - 2;
                                    for (int i = 0; i < importedResourceTableRange.RowEnd - (importedResourceTableRange.RowBegin + ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET); i++)
                                    {
                                        WorkofflineImportedResourceType importedResource = this.GetResourceTypeDataFromTableRow(importedResourceTableRange, ResourceTypeTableBegin, (ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET + i), ResourceTypeIDsProcessed, ws, TaskElement, importedTask, wsp, elements, importedBoe.IsMultiClinWbs, CustomFieldsEndColumn, importedBoe.Id, i);

                                        // if this was an empty row, throw it out
                                        if (importedResource.Id < 0 && importedResource.ImportTypes.Contains(LaborTypeImportResult.ResourceMissingInvalid)
                                            && importedResource.ImportTypes.Contains(LaborTypeImportResult.StartDateMissingInvalid) && importedResource.ImportTypes.Contains(LaborTypeImportResult.EndDateMissingInvalid) 
                                            && importedResource.ImportTypes.Contains(LaborTypeImportResult.PerfOrgMissingInvalid) && importedResource.ImportTypes.Contains(LaborTypeImportResult.SpreadCurveMissing) 
                                            && (!importedResource.ValueSpread.HasValue || importedResource.ValueSpread.Value == 0))
                                        {
                                            continue;
                                        }

                                        importedResource.BoeID = importedBoe.Id;
                                        ResourceTypeIDsProcessed.Add(importedResource.Id);
                                        importedTask.ImportedResourceTypes.Add(importedResource);

                                        // if there is a problem with the resource type id or dates or if the resource type is a spread type no need to process this resource type spreads
                                        if (importedResource.ImportTypes.Contains(LaborTypeImportResult.ResourceTypeIDMissingOrInvalid) || importedResource.ImportTypes.Contains(LaborTypeImportResult.ResourceMissingInvalid)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.NonUniqueResourceTypeID) || importedResource.ImportTypes.Contains(LaborTypeImportResult.StartDateMissingInvalid)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.EndDateMissingInvalid) || importedResource.ImportTypes.Contains(LaborTypeImportResult.LaborTypeDateSpreadDateMismatch)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.PerfOrgMissingInvalid) || importedResource.ImportTypes.Contains(LaborTypeImportResult.DecimalPrecisionViolation)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.SpreadCurveMissing) || importedResource.ImportTypes.Contains(LaborTypeImportResult.SpreadCurveInvalid)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.HoursSpreadInvalid) || importedResource.ImportTypes.Contains(LaborTypeImportResult.CostSpreadRangeInvalid)
                                            || importedResource.ImportTypes.Contains(LaborTypeImportResult.CostDecimalPrecisionViolation))
                                        {
                                            continue;
                                        }
                                        
                                        ResourceTypeDto ResourceType = null;
                                        if (importedResource.Id > 0)
                                        {
                                            ResourceType = TaskElement.taskElementLabors.FirstOrDefault(x => x.Id == importedResource.Id);
                                        }

                                        WorkofflineImportedResourceSpreadsCollection importedResourceSpreads = null;
                                        if (SpreadStartColumn >= 1000)
                                        {
                                            // if the count is 1000 that means that the start of the spreads in the resource table couldn't be found so the file was tampered with and we can abort the spread import
                                            importedResourceSpreads = new WorkofflineImportedResourceSpreadsCollection();
                                            importedResourceSpreads.ImportTypes.Add(LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange);
                                        }
                                        else
                                        {
                                            // process spreads check for discrete setting in validation so we can show a validation error if spreads exist and resource type is not set to discrete
                                            importedResourceSpreads = this.GetResourceSpreadDataFromTableRow(importedResourceTableRange, ResourceTypeTableBegin, ResourceType, importedBoe.Id, importedResource, (ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET + i), SpreadStartColumn, wsp, elements, ws.ResourceDecimalPrecision, ws.CostDecimalPrecision);
                                            
                                            if (importedResourceSpreads.ImportTypes.Contains(LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange))
                                            {
                                                importedResource.ImportTypes.Remove(LaborTypeImportResult.AddLaborType);
                                                importedResource.ImportTypes.Remove(LaborTypeImportResult.UpdateLaborType);

                                                importedResource.ImportTypes.Add(LaborTypeImportResult.LaborTypeDateSpreadDateMismatch);
                                            }
                                        }

                                        importedResource.ImportedResourceSpreads = importedResourceSpreads;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (NotExcelFileException)
            {
                importedResults.ImportTypes.Add(WorkofflineImportResult.FileError);
                // remove anything that was imported
                importedResults.ImportedBoes = new Collection<WorkofflineImportedBoe>();
            }
            catch (FileFormatException)
            {
                throw new NotExcelFileException("Imported file was an incorrect format.");
            }

            return importedResults;

        }

        private Collection<WorkofflineImportResult> ValidateWorkspaceInformation(WorkspaceDTO workspaceDTO, TableRange WorkspaceTableRange, WorksheetPart wsp, MiscSpreadsheetElements elements)
        {
            Collection<WorkofflineImportResult> toReturn = new Collection<WorkofflineImportResult>();

            Cell WorkspaceTableBegin = ExcelUtilities.GetCell(wsp, WorkspaceTableRange.Begin);

            // Get the WorkspaceID
            Cell WorkspaceIDCell = ExcelUtilities.GetCellByOffset(WorkspaceTableBegin, ImportExportConstants.WORKSPACE_ID_CELL_ROW_OFFSET, ImportExportConstants.WORKSPACE_ID_CELL_COLUMN_OFFSET, wsp);
            int importedWorkspaceID = 0;
            Boolean ParseSuccess = WorkspaceIDCell == null ? false : Int32.TryParse(ExcelUtilities.GetCellValue(WorkspaceIDCell, elements.sharedStringItems, elements.stylesheet), out importedWorkspaceID);
            if (!ParseSuccess || importedWorkspaceID == 0 || importedWorkspaceID != workspaceDTO.Id)
            {
                toReturn.Add(WorkofflineImportResult.WorkspaceIDMissingOrInvalid);
            }

            return toReturn;
        }

        private WorkofflineImportedBoe GetBoeDataFromTable(IList<int> BoeIDsProcessed, FullWorkspace workspace, TableRange BoeTableRange, WorksheetPart wsp, MiscSpreadsheetElements elements)
        {
            WorkofflineImportedBoe toReturn = new WorkofflineImportedBoe();

            Cell BoeTableBegin = ExcelUtilities.GetCell(wsp, BoeTableRange.Begin);

            // Get the BoeID
            int importedBoeID = this.GetObjectIDFromTableData(BoeTableRange, ImportExportConstants.BOE_ID_CELL_ROW_OFFSET, ImportExportConstants.BOE_ID_CELL_COLUMN_OFFSET, elements.stylesheet, elements.sharedStringItems, wsp);
            if (importedBoeID == 0)
            {
                toReturn.ImportTypes.Add(BoeImportResult.BoeIDMissingInvalid);
            }

            // Get the Boe Title
            Cell BoeTitleCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_TITLE_CELL_ROW_OFFSET, ImportExportConstants.BOE_TITLE_CELL_COLUMN_OFFSET, wsp);
            String importedBoeTitle = BoeTitleCell == null ? null : ExcelUtilities.GetCellValue(BoeTitleCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Boe Description
            Cell BoeDescriptionCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.BOE_DESCRIPTION_CELL_COLUMN_OFFSET, wsp);
            String importedBoeDescription = BoeDescriptionCell == null ? null : ExcelUtilities.GetCellValue(BoeDescriptionCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Sources Of Data
            Cell BoeSourcesOfDataCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_SOURCESOFDATA_CELL_ROW_OFFSET, ImportExportConstants.BOE_SOURCESOFDATA_CELL_COLUMN_OFFSET, wsp);
            String importedBoeSourcesOfData = BoeSourcesOfDataCell == null ? null : ExcelUtilities.GetCellValue(BoeSourcesOfDataCell, elements.sharedStringItems, elements.stylesheet);

            //Get Multi Information
            Cell BoeMultiDataCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_MULTI_CELL_ROW_OFFSET, ImportExportConstants.BOE_MULTI_CELL_COLUMN_OFFSET, wsp);
            String importedBoeMulti = BoeMultiDataCell == null ? false.ToString() : ExcelUtilities.GetCellValue(BoeMultiDataCell, elements.sharedStringItems, elements.stylesheet);


            // Get the Boe to validate and populate the WBS and CLIN since they can't change it anyway
            BoeDTO boeDTO = workspace.Boes.FirstOrDefault(i => i.Id == importedBoeID);
            
            toReturn.Id = importedBoeID;
            toReturn.Title = importedBoeTitle;
            
            // get the custom fields
            toReturn.ImportedCustomFields = this.GetRowOrientedCustomFields(BoeTableRange, BoeTableBegin, wsp, ImportExportConstants.BOE_FIRST_CUSTOMFIELD_ROW_OFFSET, ImportExportConstants.BOE_CUSTOMFIELD_LABEL_CELL_COLUMN_OFFSET, ImportExportConstants.BOE_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET, elements, workspace, importedBoeID, null, CustomFieldType.BoeDisplay);

            this.ValidateBoeInformation(BoeIDsProcessed, workspace, boeDTO, toReturn);

            if (toReturn.ImportTypes.Contains(BoeImportResult.BoeDoesNotExist))
            {
                // need to get the WBS and CLIN from the file so that the validation screen shows something
                // Get the WBS text
                Cell BoeWBSCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_WBS_CELL_ROW_OFFSET, ImportExportConstants.BOE_WBS_CELL_COLUMN_OFFSET, wsp);
                Tuple<String, String> importedWBSNumAndName = BoeWBSCell == null ? null : this.ParseWBSCLINData(ExcelUtilities.GetCellValue(BoeWBSCell, elements.sharedStringItems, elements.stylesheet));
                 
                toReturn.ImportedWBSNumber = importedWBSNumAndName.Item1;
                toReturn.ImportedWBSTitle = importedWBSNumAndName.Item2;

                // get the clin
                Cell BoeCLINCell = ExcelUtilities.GetCellByOffset(BoeTableBegin, ImportExportConstants.BOE_CLIN_CELL_ROW_OFFSET, ImportExportConstants.BOE_CLIN_CELL_COLUMN_OFFSET, wsp);
                Tuple<String, String> importedCLINNumAndName = BoeCLINCell == null ? null : this.ParseWBSCLINData(ExcelUtilities.GetCellValue(BoeCLINCell, elements.sharedStringItems, elements.stylesheet));
                toReturn.ImportedCLINNumber = importedCLINNumAndName.Item1;
                toReturn.ImportedCLINTitle = importedCLINNumAndName.Item2;
            }
            else
            {
                // if the data contains placeholder text get the data from the dto
                toReturn.Description = importedBoeDescription == null || importedBoeDescription.Equals(ImportExportConstants.PLACEHOLDER_TEXT) ? boeDTO.Description : importedBoeDescription;   // If null, field was removed from the worksheet due to ShowDescriptionAndSources being set to false.
                toReturn.DataSource = importedBoeDescription == null || importedBoeSourcesOfData.Equals(ImportExportConstants.PLACEHOLDER_TEXT) ? boeDTO.DataSource : importedBoeSourcesOfData; // If null, field was removed from the worksheet due to ShowDescriptionAndSources being set to false.
                toReturn.IsMultiClinWbs = Boolean.Parse(importedBoeMulti);
                toReturn.CLINID = boeDTO.CLINID.HasValue ? boeDTO.CLINID.Value : 0;
                toReturn.WBSID = boeDTO.WBSID.HasValue ? boeDTO.WBSID.Value : 0;
            }

            // We can't add or delete Boe's so if there are no errors its an update
            if (!toReturn.ImportTypes.Any())
            {
                toReturn.ImportTypes.Add(BoeImportResult.UpdateBoe);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets task element details imported from the spreadsheet.
        /// </summary>
        /// <param name="TaskIDsProcessed">Task element ids.</param>
        /// <param name="TaskElementTableRange">Task element details excel table.</param>
        /// <param name="Boe">The Boe.</param>
        /// <param name="wsp">The worksheet.</param>
        /// <param name="elements">Spreadsheet elements such as the shared string table.</param>
        /// <param name="workspace">Full workspace object.</param>
        /// <returns>Imported task element details.</returns>
        private WorkofflineImportedTaskElement GetTaskElementDataFromTable(IList<int> TaskIDsProcessed, TableRange TaskElementTableRange, BoeDTO Boe, WorksheetPart wsp, MiscSpreadsheetElements elements, FullWorkspace workspace)
        {
            WorkofflineImportedTaskElement toReturn = new WorkofflineImportedTaskElement();

            Cell TaskElementTableBegin = ExcelUtilities.GetCell(wsp, TaskElementTableRange.Begin);

            int importedTaskElementID = this.GetObjectIDFromTableData(TaskElementTableRange, ImportExportConstants.TASKELEMENT_ID_CELL_ROW_OFFSET, ImportExportConstants.TASKELEMENT_ID_CELL_COLUMN_OFFSET, elements.stylesheet, elements.sharedStringItems, wsp);
            if (importedTaskElementID == 0) // positive numbers are existing, negative numbers are new, zero is invalid
            {
                toReturn.ImportTypes.Add(TaskElementImportResult.TaskElementIDMissingOrInvalid);
            }
            
            // Get the Task ID 
            Cell TaskIDCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_ID_CELL_ROW_OFFSET, ImportExportConstants.TASK_ID_CELL_COLUMN_OFFSET, wsp);
            String importedTaskID = TaskIDCell == null ? null : ExcelUtilities.GetCellValue(TaskIDCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task Title 
            Cell TaskTitleCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_TITLE_CELL_ROW_OFFSET, ImportExportConstants.TASK_TITLE_CELL_COLUMN_OFFSET, wsp);
            String importedTaskTitle = TaskTitleCell == null ? null : ExcelUtilities.GetCellValue(TaskTitleCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task Description 
            Cell TaskDescriptionCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_DESCRIPTION_CELL_ROW_OFFSET, ImportExportConstants.TASK_DESCRIPTION_CELL_COLUMN_OFFSET, wsp);
            String importedTaskDescription = TaskDescriptionCell == null ? string.Empty : ExcelUtilities.GetCellValue(TaskDescriptionCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task Start Date 
            Cell TaskStartDateCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_STARTDATE_CELL_ROW_OFFSET, ImportExportConstants.TASK_STARTDATE_CELL_COLUMN_OFFSET, wsp);
            String importedTaskStartDate = TaskStartDateCell == null ? null : ExcelUtilities.GetCellValue(TaskStartDateCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task End Date 
            Cell TaskEndDateCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_ENDDATE_CELL_ROW_OFFSET, ImportExportConstants.TASK_ENDDATE_CELL_COLUMN_OFFSET, wsp);
            String importedTaskEndDate = TaskEndDateCell == null ? null : ExcelUtilities.GetCellValue(TaskEndDateCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task MOQ Hours Equation 
            Cell TaskMOQHoursEquCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_MOQHOURSEQU_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQHOURSEQU_CELL_COLUMN_OFFSET, wsp);
            String importedTaskMOQHoursEqu = TaskMOQHoursEquCell == null ? null : ExcelUtilities.GetCellValue(TaskMOQHoursEquCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task Type
            Cell TaskMOQTypeCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_MOQTYPE_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQTYPE_CELL_COLUMN_OFFSET, wsp);
            String importedTaskMOQType = TaskMOQHoursEquCell == null ? null : ExcelUtilities.GetCellValue(TaskMOQTypeCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Task Text
            Cell TaskMOQTextCell = ExcelUtilities.GetCellByOffset(TaskElementTableBegin, ImportExportConstants.TASK_MOQTEXT_CELL_ROW_OFFSET, ImportExportConstants.TASK_MOQTEXT_CELL_COLUMN_OFFSET, wsp);
            String importedTaskMOQText = TaskMOQTextCell == null ? string.Empty : ExcelUtilities.GetCellValue(TaskMOQTextCell, elements.sharedStringItems, elements.stylesheet);

            BoeTaskElementDTO Task = null;
            if (importedTaskElementID > 0)
            {
                Task = workspace.TaskElements.FirstOrDefault(t => t.Id == importedTaskElementID);
            }

            toReturn.Id = importedTaskElementID;
            toReturn.BOETaskID = importedTaskID;
            toReturn.TaskTitle = importedTaskTitle;
            if (Task == null)
            {
                toReturn.Description = importedTaskDescription.Contains(ImportExportConstants.PLACEHOLDER_TEXT) ? string.Empty : importedTaskDescription;
                toReturn.MOQText = importedTaskMOQText.Contains(ImportExportConstants.PLACEHOLDER_TEXT) ? string.Empty : importedTaskMOQText;
            }
            else
            {
                toReturn.Description = importedTaskDescription.Contains(ImportExportConstants.PLACEHOLDER_TEXT) ? Task.Description : importedTaskDescription;
                toReturn.MOQText = importedTaskMOQText.Contains(ImportExportConstants.PLACEHOLDER_TEXT) ? Task.MOQText : importedTaskMOQText;
            }

            DateTime ParsedDate;
            toReturn.StartDate = DateTime.TryParse(importedTaskStartDate, out ParsedDate) ? ParsedDate : DateTime.MinValue;
            toReturn.EndDate = DateTime.TryParse(importedTaskEndDate, out ParsedDate) ? ParsedDate : DateTime.MinValue;
            toReturn.MOQHoursEquation = importedTaskMOQHoursEqu;
            MOQTypeModelView MOQMVType = importedTaskMOQType == null ? null : this._CommonDataMapper.getMOQType().FirstOrDefault(x => x.MOQTypeName == importedTaskMOQType);
            if (MOQMVType != null)
            {
                toReturn.MOQType = (MOQType)MOQMVType.MOQTypeID;
            }
            else
            {
                toReturn.MOQType = MOQType.None;
            }

            // get the custom fields
            toReturn.ImportedCustomFields = this.GetRowOrientedCustomFields(TaskElementTableRange, TaskElementTableBegin, wsp, ImportExportConstants.TASK_FIRST_CUSTOMFIELD_ROW_OFFSET, ImportExportConstants.TASK_CUSTOMFIELD_LABEL_CELL_COLUMN_OFFSET, ImportExportConstants.TASK_CUSTOMFIELD_VALUE_CELL_COLUMN_OFFSET, elements, workspace, Boe.Id, importedTaskElementID, CustomFieldType.TaskDisplay);

            this.ValidateTaskInformation(TaskIDsProcessed, Task, Boe, toReturn, workspace);
            this.ValidateMoqEquation(Boe, toReturn, workspace);

            if (!toReturn.ImportTypes.Any())
            {
                if (toReturn.Id < 0)
                {
                    toReturn.ImportTypes.Add(TaskElementImportResult.CreateTaskElement);
                }
                if (toReturn.Id > 0)
                {
                    toReturn.ImportTypes.Add(TaskElementImportResult.UpdateTaskElement);
                }
            }

            return toReturn;
        }

        private WorkofflineImportedResourceType GetResourceTypeDataFromTableRow(TableRange ResourceTypeTableRange, Cell ResourceTypeTableBegin, int CurrentRowOffset, IList<int> ResourceTypeIDsProcessed,
            FullWorkspace Workspace, BoeTaskElementDTO TaskElement, WorkofflineImportedTaskElement importedTaskElement, WorksheetPart wsp, MiscSpreadsheetElements elements, Boolean isMulti, int CustomFieldsEnd, int importedBoeId, int rowIndex)
        {
            WorkofflineImportedResourceType toReturn = new WorkofflineImportedResourceType();

            int importedResourceTypeID = this.GetObjectIDFromTableData(ResourceTypeTableRange, (CurrentRowOffset), ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_COLUMN_OFFSET, elements.stylesheet, elements.sharedStringItems, wsp);
            if (importedResourceTypeID == 0) // positive numbers are existing, negative numbers are new, zero is invalid
            {
                toReturn.ImportTypes.Add(LaborTypeImportResult.ResourceTypeIDMissingOrInvalid);
            }

            // Get the Resource 
            Cell ResourceCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET, wsp);
            String importedResource = ResourceCell == null ? null : ExcelUtilities.GetCellValue(ResourceCell, elements.sharedStringItems, elements.stylesheet);

            // Get the PerfOrg 
            Cell PerfOrgCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET, wsp);
            String importedPerfOrg = PerfOrgCell == null ? null : ExcelUtilities.GetCellValue(PerfOrgCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Resource Start Date 
            Cell ResourceStartDateCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_STARTDATE_CELL_COLUMN_OFFSET, wsp);
            String importedResourceStartDate = ResourceStartDateCell == null ? null : ExcelUtilities.GetCellValue(ResourceStartDateCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Resource End Date 
            Cell ResourceEndDateCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_ENDDATE_CELL_COLUMN_OFFSET, wsp);
            String importedResourceEndDate = ResourceEndDateCell == null ? null : ExcelUtilities.GetCellValue(ResourceEndDateCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Spread Curve 
            Cell SpreadCurveCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_SPREADCURVE_CELL_COLUMN_OFFSET, wsp);
            String importedSpreadCurve = SpreadCurveCell == null ? null : ExcelUtilities.GetCellValue(SpreadCurveCell, elements.sharedStringItems, elements.stylesheet).Replace(FullObjectHelper.HoursLabel(Workspace), "Hours");

            // Get the % Spread
            Cell PercentSpreadCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_PERCENTSPREAD_CELL_COLUMN_OFFSET, wsp);
            String importedPercentSpread = PercentSpreadCell == null ? null : ExcelUtilities.GetCellValue(PercentSpreadCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Hours Spread
            Cell HoursSpreadCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_HOURSSPREAD_CELL_COLUMN_OFFSET, wsp);
            String importedHoursSpread = HoursSpreadCell == null ? null : ExcelUtilities.GetCellValue(HoursSpreadCell, elements.sharedStringItems, elements.stylesheet);

            // Get the Cost
            Cell CostCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET, wsp);
            String importedCost = CostCell == null ? null : ExcelUtilities.GetCellValue(CostCell, elements.sharedStringItems, elements.stylesheet);

            if (isMulti)
            {
                Cell WbsValueCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, CustomFieldsEnd, wsp);
                String importedResourceWbsValue = WbsValueCell == null ? null : ExcelUtilities.GetCellValue(WbsValueCell, elements.sharedStringItems, elements.stylesheet);

                Cell ClinValueCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, CustomFieldsEnd + 1, wsp);
                String importedResourceClinValue = ClinValueCell == null ? null : ExcelUtilities.GetCellValue(ClinValueCell, elements.sharedStringItems, elements.stylesheet);


                FullWbs wbs = Workspace.WbsElementsNoMultiWbs.FirstOrDefault(w => w.WbsString == importedResourceWbsValue);
                ClinDTO clin = Workspace.ClinsNoMultiClin.FirstOrDefault(c => c.ClinString == importedResourceClinValue);

                toReturn.WBSID = wbs != null ? wbs.Id : -1;
                toReturn.CLINID = clin != null ? clin.Id : -1;
            }
            
            toReturn.Id = importedResourceTypeID;
            ResourceDTO Resource = importedResource == null ? null : this.resourceDataLoader.GetByDescriptionAndListId(importedResource, Workspace.ResourceListID);

            toReturn.ResourceID = Resource != null ? Resource.Id : (int?)null;
            PerformingOrgDTO PerfOrg = importedPerfOrg == null ? null : this.GetPerformingDTO(Workspace.PerfOrgListID, importedPerfOrg);
            toReturn.PerformingOrgID = PerfOrg != null ? PerfOrg.Id : (int?)null;
            DateTime StartDate;
            toReturn.StartDateValue = DateTime.TryParse(importedResourceStartDate, out StartDate) ? StartDate.Normalize() : DateTime.MinValue;
            DateTime EndDate;
            toReturn.EndDateValue = DateTime.TryParse(importedResourceEndDate, out EndDate) ? EndDate.Normalize() : DateTime.MinValue;
            SpreadCurveModelView mvSpreadCurve = this._CommonDataMapper.getSpreadCurve().FirstOrDefault(x => x.SpreadCurveName == importedSpreadCurve);
            toReturn.SpreadCurveID = mvSpreadCurve != null ? mvSpreadCurve.SpreadCurveID : SpreadCurves.None;
            Decimal ParsedPercentSpread;
            toReturn.PercentSpread = Decimal.TryParse(importedPercentSpread, out ParsedPercentSpread) ? ParsedPercentSpread : (decimal?) null;
            decimal ParsedValueSpread;
            toReturn.SpreadType = SpreadType.NotSet;
            if (Resource != null && Resource.RateType == RateType.Hours)
            {
                toReturn.ValueSpread = decimal.TryParse(importedHoursSpread, out ParsedValueSpread) ? ParsedValueSpread : (decimal?)null;
                toReturn.SpreadType = SpreadType.Hours;
            }
            else if (Resource != null && Resource.RateType == RateType.Cost)
            {
                toReturn.ValueSpread = decimal.TryParse(importedCost, NumberStyles.AllowCurrencySymbol | NumberStyles.Number, CultureInfo.CurrentCulture.NumberFormat, out ParsedValueSpread) ? ParsedValueSpread : (decimal?)null;
                toReturn.SpreadType = SpreadType.Cost;
            }

            //add % Spread and Value Spread locked values
            if (Resource != null && toReturn.SpreadCurveID != SpreadCurves.DiscreteHours && Resource.RateType != RateType.Cost)
            {
                if (toReturn.PercentSpread.HasValue && !toReturn.ValueSpread.HasValue)
                {
                    toReturn.PercentSpreadLocked = true;
                    toReturn.HourSpreadLocked = false;
                }
                else if (!toReturn.PercentSpread.HasValue && toReturn.ValueSpread.HasValue)
                {
                    toReturn.HourSpreadLocked = true;
                    toReturn.PercentSpreadLocked = false;
                }
                else
                {
                    toReturn.PercentSpreadLocked = true;
                    toReturn.HourSpreadLocked = false;
                }
            }

            ResourceTypeDto ResourceType = null;
            if (toReturn.Id > 0)
            {
                ResourceType = Workspace.TaskElements.First(t => t.Id == TaskElement.Id).taskElementLabors.FirstOrDefault(x => x.Id == toReturn.Id);

                if (Resource == null && ResourceType != null && ResourceType.ResourceID.HasValue)
                {
                    Resource = Workspace.ResourcesForWsResourceListId.FirstOrDefault(r => r.Id == ResourceType.ResourceID.Value);
                }
            }

            // add custom fields 
            if (CustomFieldsEnd > (ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET + 1))
            {
                toReturn.ImportedCustomFields = this.GetColumnOrientedCustomFields(ResourceTypeTableBegin, (ImportExportConstants.FIRSTRESOURCETYPE_ID_CELL_ROW_OFFSET + rowIndex), ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET + 1, CustomFieldsEnd - 1, wsp, elements, Workspace, importedBoeId, importedTaskElement.Id, toReturn.Id);
            }

            this.ValidateResourceTypeInformation(toReturn, ResourceType, Resource, importedTaskElement, ResourceTypeIDsProcessed, Workspace.ResourceDecimalPrecision, Workspace.CostDecimalPrecision, isMulti, Workspace);

            if (!toReturn.ImportTypes.Any())
            {
                if (toReturn.Id < 0)
                {
                    toReturn.ImportTypes.Add(LaborTypeImportResult.AddLaborType);
                }
                if (toReturn.Id > 0)
                {
                    toReturn.ImportTypes.Add(LaborTypeImportResult.UpdateLaborType);
                }
            }

            return toReturn;
        }

        private WorkofflineImportedResourceSpreadsCollection GetResourceSpreadDataFromTableRow(TableRange ResourceTypeTableRange, Cell ResourceTypeTableBegin, ResourceTypeDto ResourceType, int BoeID, WorkofflineImportedResourceType importedResourceType, int CurrentRowOffset, int SpreadStartColumn, WorksheetPart wsp, MiscSpreadsheetElements elements, int? wsDecimalPrecision, int costDecimalPrecision)
        {
            WorkofflineImportedResourceSpreadsCollection toReturn = new WorkofflineImportedResourceSpreadsCollection();
            Boolean SpreadExists = false;

            int EndColumnIndex = ExcelUtilities.GetColumnIndexFromColumnName(ResourceTypeTableRange.ColumnEnd); // This call is one based we are calculating cells starting at 0;
            int Count = -1;
            for (int i = 0; i <= (EndColumnIndex - SpreadStartColumn); i++)
            {
                Cell SpreadCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, CurrentRowOffset, (SpreadStartColumn + i), wsp);
                String importedSpread = SpreadCell == null ? null : ExcelUtilities.GetCellValue(SpreadCell, elements.sharedStringItems, elements.stylesheet);
                SpreadExists = !String.IsNullOrEmpty(importedSpread) ? true : SpreadExists;

                Cell SpreadHeaderCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, ImportExportConstants.RESOURCETYPE_SPREAD_HEADER_ROW_OFFSET, (SpreadStartColumn + i), wsp);
                String importedSpreadHeader = SpreadHeaderCell == null ? null : ExcelUtilities.GetCellValue(SpreadHeaderCell, elements.sharedStringItems, elements.stylesheet);
                DateTime SpreadDate;
                if (!DateTime.TryParse(importedSpreadHeader, out SpreadDate))
                {
                    SpreadDate = DateTime.MinValue;
                }
                else
                {
                    SpreadDate = SpreadDate.Normalize();
                }
                ResourceSpreadDto ResourceSpread = null;
                if (ResourceType != null)
                {
                    ResourceSpread =
                        ResourceType.LaborSpreads.FirstOrDefault(x => DateTime.Compare(x.LaborSpreadDate.Date, SpreadDate.Date) == 0);
                }

                // SKIP non-valued spread entries that fall outside the date range for the resource type
                decimal parsedValue;
                decimal spreadValue = importedSpread != null ? decimal.TryParse(importedSpread.Replace("$", ""), out parsedValue) ? parsedValue : 0 : 0;
                if (spreadValue != 0m || this.ValidateDate(SpreadDate, importedResourceType.StartDateValue, importedResourceType.EndDateValue))
                {
                    WorkofflineImportedResourceSpread importedResourceSpread = new WorkofflineImportedResourceSpread
                    {
                        Id = ResourceType == null ? Count : ResourceSpread == null ? 0 : ResourceSpread.Id,
                        LaborSpreadDate = SpreadDate,
                        LaborSpreadValue = spreadValue,
                        BoeID = BoeID
                    };

                    toReturn.Add(importedResourceSpread);
                }
            }

            //If monthly spreads were entered, validate them
            if (SpreadExists)
            {
                this.ValidateResourceSpreadInformation(toReturn, importedResourceType, SpreadExists, wsDecimalPrecision, costDecimalPrecision);
            }

            foreach (WorkofflineImportedResourceSpread item in toReturn)
            {
                //Any ImportTypes added to the spreads would be errors, so if there aren't any, then the spreads are valid
                if (!item.ImportTypes.Any())
                {
                    item.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);
                }
                else
                {
                    foreach (LaborSpreadImportResult result in item.ImportTypes)
                    {
                        toReturn.ImportTypes.Add(result);
                    }
                }
            }

            if (!toReturn.ImportTypes.Any())
            {
                //If there were no ImportTypes added, then there were no errors found in spreads
                toReturn.ImportTypes.Add(LaborSpreadImportResult.UpdateSpread);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the row-oriented (BOE- and Task-level) Custom Fields
        /// </summary>
        /// <param name="ObjectTableRange">table range</param>
        /// <param name="ObjectTableBegin">table begin cell</param>
        /// <param name="wsp">worksheet part</param>
        /// <param name="FirstCustomFieldRowOffset">Offset for the first custom field row</param>
        /// <param name="CustomFieldLabelColumnOffset">Offset for the custom field label column</param>
        /// <param name="CustomFieldValueColumnOffset">Offset for the custom field value column</param>
        /// <param name="elements">spreadsheet elements</param>
        /// <param name="workspace">the workspace</param>
        /// <param name="boeId">the boe id</param>
        /// <param name="taskId">the task id - needed for task-level only</param>
        /// <param name="customFieldType">type of custom field</param>
        /// <returns>Collection of workoffline imported custom fields</returns>
        private Collection<WorkofflineImportedCustomField> GetRowOrientedCustomFields(TableRange ObjectTableRange, Cell ObjectTableBegin, WorksheetPart wsp, int FirstCustomFieldRowOffset, int CustomFieldLabelColumnOffset, int CustomFieldValueColumnOffset, MiscSpreadsheetElements elements, FullWorkspace workspace, int boeId, int? taskId, CustomFieldType customFieldType)
        {
            Collection<WorkofflineImportedCustomField> toReturn = new Collection<WorkofflineImportedCustomField>();

            for (int i = 0; i <= ObjectTableRange.RowEnd - ObjectTableRange.RowBegin - FirstCustomFieldRowOffset; i++)
            {
                Cell CustomFieldNameCell = ExcelUtilities.GetCellByOffset(ObjectTableBegin, FirstCustomFieldRowOffset + i, (CustomFieldLabelColumnOffset), wsp);
                String importedCustomFieldName = CustomFieldNameCell == null ? null : ExcelUtilities.RemoveCustomFieldPrefix(ExcelUtilities.GetCellValue(CustomFieldNameCell, elements.sharedStringItems, elements.stylesheet));

                Cell CustomFieldValueCell = ExcelUtilities.GetCellByOffset(ObjectTableBegin, FirstCustomFieldRowOffset + i, (CustomFieldValueColumnOffset), wsp);
                String importedCustomFieldValue = CustomFieldValueCell == null ? null : ExcelUtilities.GetCellValue(CustomFieldValueCell, elements.sharedStringItems, elements.stylesheet);
                CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName == importedCustomFieldName.TrimEnd('*'));

                int cfvId = 0;
                if (!string.IsNullOrEmpty(importedCustomFieldValue) && customField != null)
                {
                    FullBoe boe = workspace.Boes.FirstOrDefault(x => x.Id == boeId);
                    CustomFieldValueContainer customFieldValue = null;

                    if (customFieldType == CustomFieldType.BoeDisplay)
                    {
                        customFieldValue = boe != null ? boe.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id) : null;
                    }
                    else if (boe != null && customFieldType == CustomFieldType.TaskDisplay && taskId != null)
                    {
                        BoeTaskElementDTO task = boe.TaskElements.FirstOrDefault(x => x.Id == taskId);
                        customFieldValue = task != null ? task.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id) : null;
                    }

                    cfvId = customFieldValue == null ? 0 : customFieldValue.CustomFieldValueID; // this is the original Id (used for open ended)
                    if (!customField.IsOpenEnded)
                    {
                        // find the new id chosen
                        CustomFieldValueDTO cfValue = workspace.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == customField.Id && importedCustomFieldValue == (x.CustomFieldValueName + " - " + x.CustomFieldValueDescription));
                        cfvId = cfValue == null ? 0 : cfValue.CustomFieldValueID;
                    }
                }

                int cfId = customField == null ? 0 : customField.Id;
                
                WorkofflineImportedCustomField importedCustomField = new WorkofflineImportedCustomField
                {
                    ImportedCustomFieldName = importedCustomFieldName,
                    ImportedCustomFieldValueID = cfvId,
                    ImportedCustomFieldID = cfId,
                    ImportedCustomFieldIDDecription = importedCustomFieldValue ?? string.Empty,
                    IsOpenEnded = customField != null ? customField.IsOpenEnded : false
                };

                toReturn.Add(importedCustomField);
            }

            return toReturn;
        }

        /// <summary>
        /// Get the column (Resource Type Level) Custom Fields
        /// </summary>
        /// <param name="ObjectTableBegin">table begin cell</param>
        /// <param name="CurrentRowOffset">offset for the current row</param>
        /// <param name="CustomFieldStartColumn">custom field start column index</param>
        /// <param name="CustomFieldEndColumn">custom field end column index</param>
        /// <param name="wsp">worksheet parts</param>
        /// <param name="elements">spreadsheet elements</param>
        /// <param name="workspace">the workspace</param>
        /// <param name="boeId">the boe ID</param>
        /// <param name="taskId">the task ID</param>
        /// <param name="laborTypeId">the labor type ID</param>
        /// <returns>Collection of workoffline imported custom fields</returns>
        private Collection<WorkofflineImportedCustomField> GetColumnOrientedCustomFields(Cell ObjectTableBegin, int CurrentRowOffset, int CustomFieldStartColumn, 
            int CustomFieldEndColumn, WorksheetPart wsp, MiscSpreadsheetElements elements, FullWorkspace workspace, int boeId, int taskId, int laborTypeId)
        {
            Collection<WorkofflineImportedCustomField> toReturn = new Collection<WorkofflineImportedCustomField>();

            for (int i = 0; i <= (CustomFieldEndColumn - CustomFieldStartColumn); i++)
            {
                Cell CustomFieldNameCell = ExcelUtilities.GetCellByOffset(ObjectTableBegin, ImportExportConstants.RESOURCETYPE_SPREAD_HEADER_ROW_OFFSET, (CustomFieldStartColumn + i), wsp);
                String importedCustomFieldName = CustomFieldNameCell == null ? null : ExcelUtilities.RemoveCustomFieldPrefix(ExcelUtilities.GetCellValue(CustomFieldNameCell, elements.sharedStringItems, elements.stylesheet));

                Cell CustomFieldValueCell = ExcelUtilities.GetCellByOffset(ObjectTableBegin, CurrentRowOffset, (CustomFieldStartColumn + i), wsp);
                String importedCustomFieldValue = CustomFieldValueCell == null ? null : ExcelUtilities.GetCellValue(CustomFieldValueCell, elements.sharedStringItems, elements.stylesheet);
                CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName == importedCustomFieldName.TrimEnd('*'));

                int cfvId = 0;
                if (!string.IsNullOrEmpty(importedCustomFieldValue) && customField != null)
                {
                    ResourceTypeDto laborType = workspace.Boes.FirstOrDefault(x => x.Id == boeId)?.TaskElements.FirstOrDefault(x => x.Id == taskId)?.taskElementLabors.FirstOrDefault(x => x.Id == laborTypeId);
                    CustomFieldValueContainer customFieldValue = laborType != null ? laborType.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id) : null;
                    cfvId = customFieldValue == null ? 0 : customFieldValue.CustomFieldValueID; // this is the original Id (used for open ended)
                    if (!customField.IsOpenEnded)
                    {
                        // find the new id chosen
                        CustomFieldValueDTO cfValue = workspace.CustomFieldValues.FirstOrDefault(x => x.CustomFieldID == customField.Id && importedCustomFieldValue == (x.CustomFieldValueName + " - " + x.CustomFieldValueDescription));
                        cfvId = cfValue == null ? 0 : cfValue.CustomFieldValueID;
                    }
                }

                int cfId = customField == null ? 0 : customField.Id;
                
                WorkofflineImportedCustomField importedCustomField = new WorkofflineImportedCustomField
                {
                    ImportedCustomFieldName = importedCustomFieldName,
                    ImportedCustomFieldValueID = cfvId,
                    ImportedCustomFieldID = cfId,
                    ImportedCustomFieldIDDecription = importedCustomFieldValue ?? string.Empty,
                    IsOpenEnded = customField != null ? customField.IsOpenEnded : false
                };

                toReturn.Add(importedCustomField);
            }

            return toReturn;
        }

        /// <summary>
        /// Validates imported Boe Data. Adds any validation errors to the given importedBoe.
        /// </summary>
        /// <param name="BoeIDsProcessed">List of Boe Ids being imported.</param>
        /// <param name="workspace">Full workspace object.</param>
        /// <param name="boeDTO">Boe to validate from the DB.</param>
        /// <param name="importedBoe">The imported Boe data.</param>
        private void ValidateBoeInformation(IList<int> BoeIDsProcessed, FullWorkspace workspace, BoeDTO boeDTO, WorkofflineImportedBoe importedBoe)
        {
            if (boeDTO == null)
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeDoesNotExist);
                return; // no need to process any further
            }

            if (boeDTO.WorkspaceID != workspace.Id)
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeNotInWorkspace);
                return; // no need to process any further
            }

            if (boeDTO.State == BOEState.Approved || boeDTO.State == BOEState.AwaitingApproval)
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeNotInDraftState);
                return; // no need to process any further
            }

            if (BoeIDsProcessed.Contains(importedBoe.Id))
            {
                importedBoe.ImportTypes.Add(BoeImportResult.DuplicateBoeID);
            }

            if (!this.IsUserAuthor(boeDTO, workspace))
            {
                importedBoe.ImportTypes.Add(BoeImportResult.ImportingUserNotAuthor);
            }

            this.ValidateBoeTitle(importedBoe);

            if (importedBoe.DataSource != null && importedBoe.DataSource.Length > ValidationConstants.BOE_SOURCE_OF_DATA_CHAR_LIMIT)
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeSourceOfData2000CharLimit);
            }

            // check CFs
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a boe custom field and required, check to see if the boe has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.BoeDisplay)
                {
                    if (customField.IsOpenEnded)
                    {
                        WorkofflineImportedCustomField importedCustomField = importedBoe.ImportedCustomFields.FirstOrDefault(x => x.ImportedCustomFieldID == customField.Id);

                        if (importedCustomField == null || string.IsNullOrEmpty(importedCustomField.ImportedCustomFieldIDDecription))
                        {
                            importedBoe.ImportTypes.Add(BoeImportResult.MissingRequiredBOECustomField);

                            // Only need to find one missing
                            break;
                        }
                    }
                    else
                    {
                        // If there is no matching imported custom field
                        if (!importedBoe.ImportedCustomFields.Any(c => c.ImportedCustomFieldID == customField.Id))
                        {
                            importedBoe.ImportTypes.Add(BoeImportResult.MissingRequiredBOECustomField);

                            // Only need to find one missing
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the tasks being imported
        /// </summary>
        /// <param name="TaskIDsProcessed">task ids</param>
        /// <param name="TaskElement">task element</param>
        /// <param name="Boe">boe</param>
        /// <param name="importedTask">imported task element</param>
        /// <param name="workspace">workspace</param>
        private void ValidateTaskInformation(IList<int> TaskIDsProcessed, BoeTaskElementDTO TaskElement, BoeDTO Boe, WorkofflineImportedTaskElement importedTask, FullWorkspace workspace)
        {
            if (TaskElement == null && importedTask.Id >= 0)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.TaskElementIDMissingOrInvalid);
                return; // no need to process this task any further
            }

            if (TaskIDsProcessed.Contains(importedTask.Id))
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.TaskElementIDMissingOrInvalid);
                return; // no need to process this task any further
            }

            if (importedTask.BOETaskID != null && importedTask.BOETaskID.Length > 3)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.IDMax3Characters);
            }

            if (String.IsNullOrEmpty(importedTask.TaskTitle))
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.TitleRequired);
            }
            else if (importedTask.TaskTitle != null && importedTask.TaskTitle.Length > ValidationConstants.MAX_TASK_TITLE_LENGTH)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.TitleMax100Characters);
            }

            if (importedTask.StartDate.HasValue && (importedTask.StartDate.Value.CompareTo(DateTime.MinValue) == 0 || !this.ValidateDate(importedTask.StartDate, Boe.StartDate, Boe.EndDate)))
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.StartDateMissingInvalid);
            }

            if (importedTask.EndDate.HasValue && (importedTask.EndDate.Value.CompareTo(DateTime.MinValue) == 0 || !this.ValidateDate(importedTask.EndDate, Boe.StartDate, Boe.EndDate)))
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.EndDateMissingInvalid);
            }

            // validate start date is before or equal to end date
            if (importedTask.StartDate.HasValue && importedTask.EndDate.HasValue && importedTask.StartDate.Value.Date.CompareTo(importedTask.EndDate.Value.Date) > 0)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.StartDateMissingInvalid);
                importedTask.ImportTypes.Add(TaskElementImportResult.EndDateMissingInvalid);
            }

            // check CFs  
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a task custom field and required, check to see if the task element has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.TaskDisplay)
                {
                    if(customField.IsOpenEnded)
                    {
                        WorkofflineImportedCustomField importedCustomField = importedTask.ImportedCustomFields.FirstOrDefault(x => x.ImportedCustomFieldID == customField.Id);
                        
                        if (importedCustomField == null || string.IsNullOrEmpty(importedCustomField.ImportedCustomFieldIDDecription))
                        {
                            importedTask.ImportTypes.Add(TaskElementImportResult.MissingRequiredTaskCustomField);

                            // Only need to find one missing
                            break;
                        }
                    }
                    else
                    {
                        // If there is no matching imported custom field
                        if (!importedTask.ImportedCustomFields.Any(c => c.ImportedCustomFieldID == customField.Id))
                        {
                            importedTask.ImportTypes.Add(TaskElementImportResult.MissingRequiredTaskCustomField);

                            // Only need to find one missing
                            break;
                        } 
                    }
                }
            }
        }

        /// <summary>
        /// Validates the imported MOQ equation, checks for circular references, and updates the
        /// WorkspaceVariableIDs property for the task.
        /// </summary>
        /// <param name="Boe">BOE task belongs to</param>
        /// <param name="importedTask">Task being imported</param>
        /// <param name="workspace">Full workspace</param>
        private void ValidateMoqEquation(BoeDTO Boe, WorkofflineImportedTaskElement importedTask, FullWorkspace workspace)
        {
            try
            {
                ICollection<string> validationResults = Parser.Validate(importedTask.MOQHoursEquation);
                Collection<WorkspaceVariableDTO> wsVariableInUse = new Collection<WorkspaceVariableDTO>();

                //only need to do this check if variables are part of the equation
                //First element in validationResults will be reformatted equation, and any other elements are variables found in equation
                if (validationResults.Count >= 2)
                {
                    // only need to worry about sum of boe workspace variables
                    var allWorkspaceVariables = workspace.WorkspaceVariables;

                    foreach (string variable in validationResults)
                    {
                        bool createsCR = false;
                        //Find a matching WS Variable
                        var wsVar = (from a in allWorkspaceVariables
                                     where a.WorkspaceVariableName.ToUpper() == variable.ToUpper()
                                     select a).FirstOrDefault();

                        if (wsVar != null)
                        {
                            if (wsVar.ValueType == VarValueType.SumOfBOEs)
                            {
                                var circularReferenceCache = new VariableCircularReferenceCheckerCache();

                                createsCR = this._VariableCircularReferenceChecker.WorkspaceVariableCreatesCircularReference(circularReferenceCache, Boe.Id, wsVar, workspace);
                                if (createsCR)
                                {
                                    importedTask.ImportTypes.Add(TaskElementImportResult.MOQCircularReference);
                                }
                            }
                            if (!createsCR)
                            {
                                wsVariableInUse.Add(wsVar);
                            }
                        }
                    }
                    if (wsVariableInUse.Any())
                    {
                        importedTask.WorkspaceVariableIDs = wsVariableInUse.Select(x => x.Id).ToCollection();
                        importedTask.MOQHoursEquation = Parser.TagVariables(importedTask.MOQHoursEquation, wsVariableInUse);
                    }
                }
            }
            catch (GeneralMOQParsingException)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.MOQHoursEquationMissingInvalid);
            }
            catch (GeneralMOQCalculationException)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.MOQHoursEquationMissingInvalid);
            }

            if (importedTask.MOQHoursEquation.Length > ValidationConstants.MOQ_HOURS_EQUATION_LENGTH_LIMIT)
            {
                importedTask.ImportTypes.Add(TaskElementImportResult.MOQHoursEquationMax250Characters);
            }
        }

        /// <summary>
        /// Validates the resource being imported
        /// </summary>
        /// <param name="importedResourceType">the resource imported</param>
        /// <param name="ResourceType">the resource typ dto</param>
        /// <param name="Resource">the res</param>
        /// <param name="importedTaskElement">imported task element</param>
        /// <param name="ResourceTypeIDsProcessed">resource ids</param>
        /// <param name="maxDecimalPrecision">ws decimal precision</param>
        /// <param name="costDecimalPrecision">cost decimal</param>
        /// <param name="isMulti">multi boe </param>
        /// <param name="workspace">workspace</param>
        private void ValidateResourceTypeInformation(WorkofflineImportedResourceType importedResourceType, ResourceTypeDto ResourceType, ResourceDTO Resource, WorkofflineImportedTaskElement importedTaskElement,
            IList<int> ResourceTypeIDsProcessed, int? maxDecimalPrecision, int costDecimalPrecision, Boolean isMulti, FullWorkspace workspace)
        {
            if (ResourceType == null && importedResourceType.Id > 0)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.ResourceTypeIDMissingOrInvalid);
            }

            // if the resource text in the spread sheet is missing or incorrect the ID will be null
            if (importedResourceType.ResourceID == null)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.ResourceMissingInvalid);
            }

            if (ResourceTypeIDsProcessed.Contains(importedResourceType.Id))
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.NonUniqueResourceTypeID);
                return; // no need to process this resource type any further
            }

            if (importedResourceType.PerformingOrgID == null)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.PerfOrgMissingInvalid);
            }

            //Make sure Resource Type Hours Spread does not exceed maximum/minimum values allowed and does not violate Decimal Precision setting
            //Value is ignored and not imported if a discrete curve is selected
            if (importedResourceType.SpreadType == SpreadType.Hours && importedResourceType.SpreadCurveID != SpreadCurves.DiscreteHours && importedResourceType.ValueSpread.HasValue)
            {
                if (!ImportUtils.isHoursWithinLimits(importedResourceType.ValueSpread))
                {
                    importedResourceType.ImportTypes.Add(LaborTypeImportResult.HoursSpreadInvalid);
                }
                else
                {
                    decimal hoursSpread;
                    //Make sure the spread values are not more precise than the workspace setting for Decimal Precision allows
                    if (!ImportUtils.IsMaxDecimalPlaces(importedResourceType.ValueSpread.ToString(), maxDecimalPrecision, out hoursSpread))
                    {
                        importedResourceType.ImportTypes.Add(LaborTypeImportResult.DecimalPrecisionViolation);
                    }
                }
            }
            else if (importedResourceType.SpreadType == SpreadType.Cost && importedResourceType.SpreadCurveID != SpreadCurves.DiscreteCost && importedResourceType.ValueSpread.HasValue)
            {
                if (!ImportUtils.isCostWithinLimits(importedResourceType.ValueSpread))
                {
                    importedResourceType.ImportTypes.Add(LaborTypeImportResult.CostSpreadRangeInvalid);
                }
                else
                {
                    decimal costSpread;
                    //Make sure the spread values are not more precise than the workspace setting for Decimal Precision allows
                    if (!ImportUtils.IsMaxDecimalPlacesForCost(importedResourceType.ValueSpread.ToString(), costDecimalPrecision, out costSpread))
                    {
                        importedResourceType.ImportTypes.Add(LaborTypeImportResult.CostDecimalPrecisionViolation);
                    }
                }
            }


            if (importedResourceType.StartDateValue == DateTime.MinValue || importedResourceType.StartDateValue == DateTime.MaxValue || !this.ValidateDate(importedResourceType.StartDateValue, importedTaskElement.StartDate.Value, importedTaskElement.EndDate.Value))
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.StartDateMissingInvalid);
            }

            if (importedResourceType.EndDateValue == DateTime.MinValue || !this.ValidateDate(importedResourceType.EndDateValue, importedTaskElement.StartDate.Value, importedTaskElement.EndDate.Value))
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.EndDateMissingInvalid);
            }

            // validate start date is before or equal to end date
            if (importedResourceType.StartDate.HasValue && importedResourceType.EndDate.HasValue && importedResourceType.StartDate.Value.Date.CompareTo(importedResourceType.EndDate.Value.Date) > 0)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.StartDateMissingInvalid);
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.EndDateMissingInvalid);
            }

            if (importedResourceType.SpreadCurveID == SpreadCurves.None)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.SpreadCurveMissing);
            }
            else
            {
                if ((Resource !=null) && ((importedResourceType.SpreadCurveID == SpreadCurves.DiscreteCost && Resource.RateType == RateType.Hours) ||
                   (importedResourceType.SpreadCurveID == SpreadCurves.DiscreteHours && Resource.RateType == RateType.Cost)))
                {
                    //Flag error because either an hours type resource has Discrete Cost selected, or cost type resource has hours curve selected
                    importedResourceType.ImportTypes.Add(LaborTypeImportResult.SpreadCurveInvalid);
                }
            }
            if (isMulti && importedResourceType.WBSID < 0 && importedResourceType.CLINID < 0)
            {
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.ResourceMultiValuesInvalid);
            }

            // check CFs
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a labor type custom field and required, check to see if the labor type has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                {
                    if(customField.IsOpenEnded)
                    {
                        WorkofflineImportedCustomField importedCustomField = importedResourceType.ImportedCustomFields.FirstOrDefault(x => x.ImportedCustomFieldID == customField.Id);

                        if (importedCustomField == null || string.IsNullOrEmpty(importedCustomField.ImportedCustomFieldIDDecription))
                        {
                            importedResourceType.ImportTypes.Add(LaborTypeImportResult.MissingRequiredResourceCustomField);

                            // Only need to find one missing
                            break;
                        }
                    }
                    else
                    {
                        // If there is no matching imported custom field
                        if (!importedResourceType.ImportedCustomFields.Any(c => c.ImportedCustomFieldID == customField.Id))
                        {
                            importedResourceType.ImportTypes.Add(LaborTypeImportResult.MissingRequiredResourceCustomField);

                            // Only need to find one missing
                            break;
                        } 
                    }
                }
            }
        }


        private void ValidateResourceSpreadInformation(WorkofflineImportedResourceSpreadsCollection importedResourceSpreads, WorkofflineImportedResourceType importedResourceType, Boolean SpreadExists, int? maxDecimalPrecision, int costDecimalPrecision)
        {
            decimal valueTotal = 0;

            //If a Discrete spread was not selected, no need to continue processing
            if (!(importedResourceType.SpreadCurveID == SpreadCurves.DiscreteCost || importedResourceType.SpreadCurveID == SpreadCurves.DiscreteHours) && SpreadExists)
            {
                importedResourceSpreads.ImportTypes.Add(LaborSpreadImportResult.DiscreteWhereCurveExpected);
                return; // no need to continue processing
            }

            foreach(WorkofflineImportedResourceSpread item in importedResourceSpreads)
            {
                //Hour type validations
                if (importedResourceType.SpreadType == SpreadType.Hours)
                {
                    if(!ImportUtils.isHoursWithinLimits(item.LaborSpreadValue))
                    {
                        item.ImportTypes.Add(LaborSpreadImportResult.HoursSpreadRangeInvalid);
                    }
                    else 
                    {
                        decimal hoursSpread;
                        //Make sure the spread values are not more precise than the workspace setting for Decimal Precision allows
                        if (!ImportUtils.IsMaxDecimalPlaces(item.LaborSpreadValue.ToString(), maxDecimalPrecision, out hoursSpread))
                        {
                            item.ImportTypes.Add(LaborSpreadImportResult.DecimalPrecisionViolation);
                        }
                    }
                }
                //Cost Type validations
                else if (importedResourceType.SpreadType == SpreadType.Cost && importedResourceType.SpreadCurveID == SpreadCurves.DiscreteCost)
                {
                    // Only interested in discrete cost spread values because they are summed to make the total cost. When cost is spread over a curve, imported
                    // spreads for that resource are ignored and re-spread using the "Cost" value.
                    if (!ImportUtils.isCostWithinLimits(item.LaborSpreadValue))
                    {
                        item.ImportTypes.Add(LaborSpreadImportResult.CostSpreadRangeInvalid);
                    }
                    else
                    {
                        decimal costSpread;
                        // Make sure the spread values are not more precise than the workspace setting for cost decimal precision.
                        if (!ImportUtils.IsMaxDecimalPlacesForCost(item.LaborSpreadValue.ToString(), costDecimalPrecision, out costSpread))
                        {
                            item.ImportTypes.Add(LaborSpreadImportResult.CostDecimalPrecisionViolation);
                        }
                    }
                }

                valueTotal = valueTotal + item.LaborSpreadValue;

                if (!this.ValidateDate(item.LaborSpreadDate, importedResourceType.StartDateValue, importedResourceType.EndDateValue))
                {
                    if (item.LaborSpreadValue != 0m)  // if no value specified, then skip date range validation error (this will be skipped when import is applied)
                    {
                        item.ImportTypes.Add(LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange);
                    }
                }
            }

            //Make sure sum of all spreads for a resource type falls within limits allowed
            if (importedResourceType.SpreadType == SpreadType.Hours)
            {
                if(!ImportUtils.isHoursWithinLimits(valueTotal))
                {
                    importedResourceSpreads.ImportTypes.Add(LaborSpreadImportResult.HoursSpreadRangeInvalid);
                }
            }
            else
            {
                if(!ImportUtils.isCostWithinLimits(valueTotal))
                {
                    importedResourceSpreads.ImportTypes.Add(LaborSpreadImportResult.CostSpreadRangeInvalid);
                }
            }

            if (importedResourceSpreads.Any(x => x.ImportTypes.Contains(LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange)))
            {
                importedResourceSpreads.ImportTypes.Add(LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange);
            }

            if (importedResourceSpreads.SelectMany(e => e.ImportTypes).Any(e => e != LaborSpreadImportResult.UpdateSpread))
            {
                // If there are any errors with this set of resource spreads, do not attempt to import the resource itself.
                if (importedResourceType.ImportTypes.Contains(LaborTypeImportResult.UpdateLaborType))
                {
                    // Remove the update resource trigger type.
                    importedResourceType.ImportTypes.Remove(LaborTypeImportResult.UpdateLaborType);
                }
                importedResourceType.ImportTypes.Add(LaborTypeImportResult.ResourceSpreadsInvalid);
            }
        }

        /// <summary>
        /// Validates a date is within (inclusive) Start and End
        /// </summary>
        /// <param name="importedDate">The date to validate</param>
        /// <param name="Start">The low end of range</param>
        /// <param name="End">The high end of range</param>
        /// <returns>True if date is within range false otherwise</returns>
        private Boolean ValidateDate(DateTime? importedDate, DateTime Start, DateTime End)
        {
            Boolean toReturn = false;

            if (!importedDate.HasValue)
            {
                return toReturn;
            }
            DateTime AdjustedImportedDate = GenBOEUtilities.AdjustDateTimePrecision(importedDate.Value.Date);
            DateTime AdjustedStartDate = GenBOEUtilities.AdjustDateTimePrecision(Start);
            DateTime AdjustedEndDate = GenBOEUtilities.AdjustDateTimePrecision(End);

            if (AdjustedImportedDate.CompareTo(AdjustedStartDate) >= 0 && AdjustedImportedDate.CompareTo(AdjustedEndDate) <= 0)
            {
                toReturn = true;
            }

            return toReturn;
        }

        protected virtual void ValidateBoeTitle(WorkofflineImportedBoe importedBoe)
        {
            if (importedBoe == null)
            {
                throw new ArgumentNullException(nameof(importedBoe), "Parameter importedBoe is required.");
            }

            if (importedBoe.Title != null && importedBoe.Title.Length > ValidationConstants.MAX_BOE_TITLE_LENGTH)
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeTitle100CharLimit);
            }
        }

        /// <summary>
        /// Determines if the current user is assigned as an author.
        /// </summary>
        /// <param name="boeDTO">Boe</param>
        /// <param name="workspace">Fullworkspace object.</param>
        /// <returns>True if the user is an author of the given Boe.</returns>
        protected virtual Boolean IsUserAuthor(BoeDTO boeDTO, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (boeDTO == null)
            {
                throw new ArgumentNullException(nameof(boeDTO), "boeDTO can't be null");
            }
            Boolean toReturn = false;
            UserDTO activeUser = workspace.CurrentActiveUser;
            
            // retrieve the list of authors on the BOE
            var authorsList = from x in this._IPermissionsDTOLoader.GetBOEPermissions(new List<int>() { boeDTO.Id })
                              where x.Role == Role.Author || x.Role == Role.SubcontractorAuthor
                              select x.ETIUserId;

            toReturn = authorsList.Contains(activeUser.UserID);   // check if the current user is assigned as an author
            return toReturn;
        }

        private String GetTableSequenceNumber(String TableName)
        {
            return TableName.Substring(TableName.LastIndexOf("_") + 1);
        }

        private Tuple<String, String> ParseWBSCLINData(String WBSCLINString)
        {
            string[] parsedData = WBSCLINString.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            string number = string.Empty;
            string title = string.Empty;
            //if there were not more than two elements in the field, data was invalid, just use default string.empty values for return tuple
            // since a colon is a valid title, join everything after the first parsed data element
            if (parsedData.Length > 2)
            {
                number = parsedData[0];
                title = string.Join(":", parsedData.Skip(1));
            }
            else
            {
                number = WBSCLINString ?? string.Empty;
            }

            return new Tuple<String, String>(number, title);
        }

        private int GetObjectIDFromTableData(TableRange ObjectTableRange, int RowOffset, int ColumnOffset, Stylesheet stylesheet, SharedStringItem[] sharedStringItems, WorksheetPart wsp)
        {
            Cell ObjectTableBegin = ExcelUtilities.GetCell(wsp, ObjectTableRange.Begin);

            // Get the TaskElementID
            Cell ObjectIDCell = ExcelUtilities.GetCellByOffset(ObjectTableBegin, RowOffset, ColumnOffset, wsp);
            int importedObjectID;
            if (!Int32.TryParse(ObjectIDCell == null ? null : ExcelUtilities.GetCellValue(ObjectIDCell, sharedStringItems, stylesheet), out importedObjectID))
            {
                importedObjectID = 0;
            }
            return importedObjectID;
        }

        private PerformingOrgDTO GetPerformingDTO(int ListID, String NameAndDescription)
        {
            PerformingOrgDTO toReturn = null;
            if(NameAndDescription.Contains(" - "))
            {
                //Parse the description - name combination
                int dashIndex = NameAndDescription.IndexOf(" - ");
                string performingOrgName = String.Empty;
                if (dashIndex > 0)
                {
                    performingOrgName = NameAndDescription.Substring(0, dashIndex);
                }
                string performingOrgDescription = String.Empty;
                if (dashIndex + 3 <= NameAndDescription.Length)
                {
                    performingOrgDescription = NameAndDescription.Substring(dashIndex + 3);
                }

                // lets look by name first
                Collection<PerformingOrgDTO> perfOrgs = this.perfOrgLoader.GetByListIdAndPartialNameOrDescription(ListID, performingOrgName);
                toReturn = perfOrgs.FirstOrDefault(x => x.PerformingOrgName == performingOrgName && x.PerformingOrgDesc == performingOrgDescription);

                // if we don't find one look by description before we return null
                if (toReturn == null)
                {
                    perfOrgs = this.perfOrgLoader.GetByListIdAndPartialNameOrDescription(ListID, performingOrgDescription);
                    toReturn = perfOrgs.FirstOrDefault(x => x.PerformingOrgName == performingOrgName && x.PerformingOrgDesc == performingOrgDescription);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Finds the column index where the spreads start and returns the offset number from the end of the resource type
        /// </summary>
        /// <param name="ResourceTypeTableBegin">The resource type table begin.</param>
        /// <param name="wsp">The WSP.</param>
        /// <param name="elements">The elements.</param>
        /// <returns></returns>
        private int FindSpreadStartColumnOffset(Cell ResourceTypeTableBegin, WorksheetPart wsp, MiscSpreadsheetElements elements)
        {
            Boolean success = false;
            int count = 0;
            do
            {
                count++;
                Cell SpreadHeaderCell = ExcelUtilities.GetCellByOffset(ResourceTypeTableBegin, ImportExportConstants.RESOURCETYPE_SPREAD_HEADER_ROW_OFFSET, (ImportExportConstants.RESOURCETYPE_COST_CELL_COLUMN_OFFSET + count), wsp);
                String importedSpreadHeader = SpreadHeaderCell == null ? null : ExcelUtilities.GetCellValue(SpreadHeaderCell, elements.sharedStringItems, elements.stylesheet);
                DateTime SpreadDate = DateTime.MinValue;
                success = DateTime.TryParse(importedSpreadHeader, out SpreadDate);
            }
            while (!success && count < 1000); // we use count as a safety out in case the spreadsheet has been changed by the user and there are no strings in the header that will convert to a date.

            return count;
        }

        private MiscSpreadsheetElements GetMiscSpreadsheetElements(SpreadsheetDocument document)
        {
            Stylesheet ss = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            SharedStringTablePart shareStringPart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            SharedStringItem[] ssi = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();  // TODO:SJR - Out of Memory exception HERE

            return new MiscSpreadsheetElements()
            {
                stylesheet = ss,
                sharedStringItems = ssi
            }; 
        }
   }

    public class MiscSpreadsheetElements
    {
        public Stylesheet stylesheet { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SharedStringItem[] sharedStringItems { get; set; }
    }

    public enum WorkofflineImportResult
    {
        None = 0,
        WorkspaceIDMissingOrInvalid = 1,
        FileError = 2,
        RuntimeException = 3
    }

    public enum TaskElementImportResult
    {
        None = 0,
        CreateTaskElement = 1,
        UpdateTaskElement = 2,
        DeleteTaskElement = 3,
        IDMax3Characters = 4,
        TitleRequired = 5,
        TitleMax100Characters = 6,
        StartDateMissingInvalid = 7,
        EndDateMissingInvalid = 8,
        MOQHoursEquationMissingInvalid = 9,
        MOQHoursEquationMax250Characters = 10,
        NonUniqueTaskID = 11,
        TaskElementIDMissingOrInvalid = 12,
        MOQCircularReference = 13,
        MissingRequiredTaskCustomField = 14 
    }

    public class WorkofflineImport
    {
        public WorkofflineImport()
        {
            this.ImportTypes = new Collection<WorkofflineImportResult>();
            this.ImportedBoes = new Collection<WorkofflineImportedBoe>();
        }
        public Collection<WorkofflineImportResult> ImportTypes { get; set; }
        public Collection<WorkofflineImportedBoe> ImportedBoes { get; set; }
    }

    public class WorkofflineImportedBoe : BoeDTO
    {
        public WorkofflineImportedBoe()
        {
            this.ImportTypes = new Collection<BoeImportResult>();
            this.ImportedTaskElements = new Collection<WorkofflineImportedTaskElement>();
            this.ImportedCustomFields = new Collection<WorkofflineImportedCustomField>();
            this.ImportedWBSNumber = String.Empty;
            this.ImportedWBSTitle = String.Empty;
            this.ImportedCLINNumber = String.Empty;
            this.ImportedCLINTitle = String.Empty;
        }
        public Collection<BoeImportResult> ImportTypes { get; set; }
        public Collection<WorkofflineImportedTaskElement> ImportedTaskElements { get; set; }
        public Collection<WorkofflineImportedCustomField> ImportedCustomFields { get; set; }
        public String ImportedWBSNumber { get; set; }
        public String ImportedWBSTitle { get; set; }
        public String ImportedCLINNumber { get; set; }
        public String ImportedCLINTitle { get; set; }

        public void CloneImportedCustomFieldsToContainer()
        {
            if (this.ImportedCustomFields != null)
            {
                foreach (WorkofflineImportedCustomField wocf in this.ImportedCustomFields)
                {
                    CustomFieldValueContainer cf = new CustomFieldValueContainer();
                    cf.ContainerID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldValueID = wocf.ImportedCustomFieldValueID;
                    cf.IsOpenEnded = wocf.IsOpenEnded;
                    cf.OpenEndedValue = wocf.ImportedCustomFieldIDDecription;
                    this.CustomFieldValueContainers.Add(cf);
                }
            }
        }
    }

    public class WorkofflineImportedTaskElement : BoeTaskElementDTO
    {
        public WorkofflineImportedTaskElement()
        {
            this.ImportTypes = new Collection<TaskElementImportResult>();
            this.ImportedResourceTypes = new Collection<WorkofflineImportedResourceType>();
            this.ImportedCustomFields = new Collection<WorkofflineImportedCustomField>();
        }
        public Collection<TaskElementImportResult> ImportTypes { get; set; }
        public Collection<WorkofflineImportedResourceType> ImportedResourceTypes { get; set; }
        public Collection<WorkofflineImportedCustomField> ImportedCustomFields { get; set; }

        public void CloneImportedCustomFieldsToContainer()
        {
            if (this.ImportedCustomFields != null)
            {
                foreach (WorkofflineImportedCustomField wocf in this.ImportedCustomFields)
                {
                    CustomFieldValueContainer cf = new CustomFieldValueContainer();
                    cf.ContainerID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldValueID = wocf.ImportedCustomFieldValueID;
                    cf.IsOpenEnded = wocf.IsOpenEnded;
                    cf.OpenEndedValue = wocf.ImportedCustomFieldIDDecription;
                    this.CustomFieldValueContainers.Add(cf);
                }
            }
        }
    }

    public class WorkofflineImportedResourceType : ResourceTypeDto
    {
        public WorkofflineImportedResourceType()
        {
            this.ImportTypes = new Collection<LaborTypeImportResult>();
            this.ImportedResourceSpreads = new WorkofflineImportedResourceSpreadsCollection();
            this.ImportedCustomFields = new Collection<WorkofflineImportedCustomField>();
        }
        public Collection<LaborTypeImportResult> ImportTypes { get; set; }
        public WorkofflineImportedResourceSpreadsCollection ImportedResourceSpreads { get; set; }
        public Collection<WorkofflineImportedCustomField> ImportedCustomFields { get; set; }

        public void CloneImportedCustomFieldsToContainer()
        {
            if (this.ImportedCustomFields != null)
            {
                foreach (WorkofflineImportedCustomField wocf in this.ImportedCustomFields)
                {
                    CustomFieldValueContainer cf = new CustomFieldValueContainer();
                    cf.ContainerID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldID = wocf.ImportedCustomFieldID;
                    cf.CustomFieldValueID = wocf.ImportedCustomFieldValueID;
                    cf.IsOpenEnded = wocf.IsOpenEnded;
                    cf.OpenEndedValue = wocf.ImportedCustomFieldIDDecription;
                    this.CustomFieldValueContainers.Add(cf);
                }
            }
        }
    }

    public class WorkofflineImportedResourceSpread : ResourceSpreadDto
    {
        public WorkofflineImportedResourceSpread()
        {
            this.ImportTypes = new Collection<LaborSpreadImportResult>();
        }
        public Collection<LaborSpreadImportResult> ImportTypes { get; set; }
    }

    public class WorkofflineImportedResourceSpreadsCollection : Collection<WorkofflineImportedResourceSpread>
    {
        public WorkofflineImportedResourceSpreadsCollection()
        {
            this.ImportTypes = new Collection<LaborSpreadImportResult>();
        }
        public Collection<LaborSpreadImportResult> ImportTypes { get; set; }
    }

    public class WorkofflineImportedCustomField
    {
        public WorkofflineImportedCustomField()
        {
            this.ImportedCustomFieldName = String.Empty;
            this.ImportedCustomFieldIDDecription = String.Empty;
            this.ImportedCustomFieldID = 0;
            this.ImportedCustomFieldValueID = 0;
            this.IsOpenEnded = false;
        }

        public String ImportedCustomFieldName { get; set; }
        public String ImportedCustomFieldIDDecription { get; set; }
        public int ImportedCustomFieldID { get; set; }
        public int ImportedCustomFieldValueID { get; set; }
        public bool IsOpenEnded { get; set; }
    }
}
