// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Class for importing MOQ Tables from an excel file
    /// </summary>
    public class MoqTableImporter : IMoqTableImporter
    {
        private Logger log = new Logger(typeof(MoqTableImporter));

        #region Constants

        internal const string TABLE_NAME = "Table Name";
        internal const string REPOSITORY_NAME = "Repository Name";
        internal const string QUERY_TYPE = "Query Type (Weekly/Monthly)";
        internal const string DATE_OF_REPORT = "Date of Report";
        internal const string HISTORICAL_PROGRAM_NAME = "Historical Program Name";
        internal const string WBS_ELEMENT = "WBS/WBS Element";
        internal const string START_DATE = "Period of Performance (PoP): Start Date";
        internal const string END_DATE = "Period of Performance (PoP): End Date";
        internal const string EMPLOYEE_ID_FILTERS = "Employee ID Filters";
        internal const string TOTAL_RELEVANT_HOURS_SSC = "Total Relevant Hours After Employee ID Filters Applied";
        internal const string CONTRACT_NUMBER = "Contract Number";
        internal const string TOTAL_WBS_HOURS = "Total WBS/WBS Element Hours";
        internal const string ADDITIONAL_QUERY_FILTERS = "Additional Query Filters (i.e. cost number, employee id)";
        internal const string TOTAL_RELEVANT_HOURS_RMS = "Total Relevant Hours After Additional Query Filters Applied";

        internal const string IMPORT_TAB = "MOQ Tables";

        protected virtual List<string> REQUIRED_COLUMNS  => new List<string> { TABLE_NAME, REPOSITORY_NAME, QUERY_TYPE, DATE_OF_REPORT, HISTORICAL_PROGRAM_NAME, WBS_ELEMENT, START_DATE, END_DATE, EMPLOYEE_ID_FILTERS, TOTAL_RELEVANT_HOURS_SSC };

        #endregion

        /// <summary>
        /// construtor
        /// </summary>
        public MoqTableImporter()
        {
        }

        /// <summary>
        /// Import MOQ Tables from Excel file
        /// </summary>
        /// <param name="excelFileStream">Excel file stream</param>
        /// <param name="ws">workspace</param>
        /// <returns>imported MOQ Tables</returns>
        public ICollection<ImportedMoqTable> ImportMoqTableFromExcelFile(Stream excelFileStream, FullWorkspace ws)
        {
            _ = excelFileStream ?? throw new ArgumentNullException(nameof(excelFileStream));
            _ = ws ?? throw new ArgumentNullException(nameof(ws));

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                try
                {
                    ICollection<ImportedMoqTable> results = new Collection<ImportedMoqTable>();

                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        IList<string> allColumns = this.REQUIRED_COLUMNS;
                        IList<string> requiredColumns = this.REQUIRED_COLUMNS;

                        ICollection<CustomFieldDTO> customFields = ws.CustomFields.Where(x => x.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay).ToCollection();
                        foreach(CustomFieldDTO cf in customFields)
                        {
                            if(cf.CustomFieldRequired)
                            {
                                requiredColumns.Add(ExcelUtilities.SetPrefixCustomFieldRequired(cf.CustomFieldName));
                                allColumns.Add(ExcelUtilities.SetPrefixCustomFieldRequired(cf.CustomFieldName));
                            }
                            else
                            {
                                allColumns.Add(ExcelUtilities.SetPrefixCustomField(cf.CustomFieldName));
                            }
                        }

                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, IMPORT_TAB, requiredColumns.ToArray(), allColumns.ToArray());

                        results = this.CreateImportedMoqTables(allRows, customFields);
                    }

                    return results;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        /// <summary>
        /// Create the MOQ Tables for the import
        /// </summary>
        /// <param name="allRows">all rows from the import file</param>
        /// <param name="customFields">MOQ Table custom fields</param>
        /// <returns>Imported MOQ Table data</returns>
        private ICollection<ImportedMoqTable> CreateImportedMoqTables(ICollection<Dictionary<string, string>> allRows, ICollection<CustomFieldDTO> customFields)
        {
            ICollection<ImportedMoqTable> toReturn = new Collection<ImportedMoqTable>();

            if (allRows.Any())
            {
                int newMoqTableIndex = -1;

                foreach(Dictionary<string, string> row in allRows)
                {
                    toReturn.Add(this.ConstructAndValidateMoqTable(row, customFields, newMoqTableIndex));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Construct the MOQ Table from the imported row and validate the data
        /// </summary>
        /// <param name="row">Row from the import file</param>
        /// <param name="customFields">MOQ Table Custom Fields</param>
        /// <param name="index">new ID index</param>
        /// <returns>Imported MOQ Table for the row</returns>
        private ImportedMoqTable ConstructAndValidateMoqTable(Dictionary<string, string> row, ICollection<CustomFieldDTO> customFields, int index)
        {
            _ = row ?? throw new ArgumentNullException(nameof(row));

            ImportedMoqTable toReturn = new ImportedMoqTable() { Id = index-- };

            this.ImportCompanySpecificMoqTableData(toReturn, row);

            // Table Name
            if (row.ContainsKey(TABLE_NAME) && !string.IsNullOrEmpty(row[TABLE_NAME]))
            {
                toReturn.TableName = row[TABLE_NAME];
                if (toReturn.TableName.Length > Constants.MOQ_TYPE_TEXT_FIELD_LENGTH)
                {
                    toReturn.ImportTypes.Add(MoqTableImportType.LargeTableName);
                }
            }
            else
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingTableName);
            }            

            // Date of Report
            if (row.ContainsKey(DATE_OF_REPORT) && !string.IsNullOrEmpty(row[DATE_OF_REPORT]))
            {
                if(DateTime.TryParse(row[DATE_OF_REPORT], out DateTime dateOfReport))
                {
                    toReturn.DateOfReport = dateOfReport;
                }
                else
                {
                    toReturn.ImportTypes.Add(MoqTableImportType.InvalidDateOfReport);
                }
            }
            else if (!toReturn.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // Historical Program Name
            if (row.ContainsKey(HISTORICAL_PROGRAM_NAME) && !string.IsNullOrEmpty(row[HISTORICAL_PROGRAM_NAME]))
            {
                toReturn.HistoricalProgramName = row[HISTORICAL_PROGRAM_NAME];
                if (toReturn.HistoricalProgramName.Length > Constants.MOQ_HISTORICAL_PROG_NAME_FIELD_LENGTH)
                {
                    toReturn.ImportTypes.Add(MoqTableImportType.LargeHistoricalProgramName);
                }
            }
            else if (!toReturn.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // WBS/WBS Element
            if (row.ContainsKey(WBS_ELEMENT) && !string.IsNullOrEmpty(row[WBS_ELEMENT]))
            {
                toReturn.WbsElement = row[WBS_ELEMENT];
                if (toReturn.WbsElement.Length > Constants.MOQ_WBS_ELEMENT_SSC_FIELD_LENGTH) // todo make sure to do for rms
                {
                    toReturn.ImportTypes.Add(MoqTableImportType.LargeWBSElement);
                }
            }
            else if (!toReturn.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // PoP Start Date
            DateTime popStartDate = new DateTime();
            if (row.ContainsKey(START_DATE) && !string.IsNullOrEmpty(row[START_DATE]))
            {
                if (toReturn.QueryType == MoqTableData.WEEKLY)
                {
                    if (IsFiscalWeekValid(row[START_DATE], out int weekValue, out int yearValue))
                    { 
                        toReturn.PoPStartWeek = weekValue;
                        toReturn.PoPStartYear = yearValue;
                        if (yearValue > DateTime.Now.Year)
                        {
                            toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopStart);
                        }
                    }
                    else
                    {
                        toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopStartFW);
                    }
                }
                else
                {
                    bool validStartDate = DateTime.TryParse(row[START_DATE], out popStartDate);
                    if (validStartDate && popStartDate <= DateTime.Now)
                    {
                        toReturn.PoPStart = popStartDate;
                    }
                    else
                    {
                        toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopStart);
                    }
                }
            }
            else if (!toReturn.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // PoP End Date
            DateTime popEndDate = new DateTime();
            if (row.ContainsKey(END_DATE) && !string.IsNullOrEmpty(row[END_DATE]))
            {
                if (toReturn.QueryType == MoqTableData.WEEKLY)
                {
                    if (IsFiscalWeekValid(row[END_DATE], out int weekValue, out int yearValue))
                    {
                        toReturn.PoPEndWeek = weekValue;
                        toReturn.PoPEndYear = yearValue;
                        if (yearValue > DateTime.Now.Year)
                        {
                            toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopEnd);
                        }
                    }
                    else
                    {
                        toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopEndFW);
                    }
                }
                else
                {
                    bool validEndDate = DateTime.TryParse(row[END_DATE], out popEndDate);
                    if (validEndDate && popEndDate <= DateTime.Now)
                    {
                        toReturn.PoPEnd = popEndDate;
                    }
                    else
                    {
                        toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopEnd);
                    }
                }
            }
            else if (!toReturn.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                toReturn.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // Validate PoP date range
            if(toReturn.PoPStart != DateTime.MinValue && toReturn.PoPEnd != DateTime.MinValue && toReturn.PoPEnd < toReturn.PoPStart)
            {
                toReturn.ImportTypes.Add(MoqTableImportType.InvalidPopRange);
            }

            this.ImportCustomFields(toReturn, row, customFields, ref index);

            // If no import types, there are no issues, so add CreateMoqTable import type
            if(!toReturn.ImportTypes.Any())
            {
                toReturn.ImportTypes.Add(MoqTableImportType.CreateMoqTable);
            }

            return toReturn;
        }

        /// <summary>
        /// Populate the company specific fields for the MOQ Table
        /// </summary>
        /// <param name="moqTable">The imported MOQ Table</param>
        /// <param name="row">row from the import file</param>
        protected virtual void ImportCompanySpecificMoqTableData(ImportedMoqTable moqTable, Dictionary<string, string> row)
        {
            _ = moqTable ?? throw new ArgumentNullException(nameof(moqTable));
            _ = row ?? throw new ArgumentNullException(nameof(row));

            // Repository Name
            if (row.ContainsKey(REPOSITORY_NAME) && !string.IsNullOrEmpty(row[REPOSITORY_NAME]))
            {
                moqTable.RepositoryName = row[REPOSITORY_NAME];
                if (moqTable.RepositoryName.Length > Constants.MOQ_REPOSITORY_NAME_FIELD_LENGTH)
                {
                    moqTable.ImportTypes.Add(MoqTableImportType.LargeRepositoryName);
                }
            }
            else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // Query Type
            if (row.ContainsKey(QUERY_TYPE) && !string.IsNullOrEmpty(row[QUERY_TYPE]))
            {
                if (row[QUERY_TYPE] == MoqTableData.WEEKLY || row[QUERY_TYPE] == MoqTableData.MONTHLY)
                {
                    moqTable.QueryType = row[QUERY_TYPE];
                }
                else
                {
                    moqTable.ImportTypes.Add(MoqTableImportType.InvalidQueryType);
                }
            }
            else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // Employee ID Filters
            if (row.ContainsKey(EMPLOYEE_ID_FILTERS) && !string.IsNullOrEmpty(row[EMPLOYEE_ID_FILTERS]))
            {
                moqTable.AdditionalQueryFilters = row[EMPLOYEE_ID_FILTERS];
            }
            else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // Total Relevant Hours
            if (row.ContainsKey(TOTAL_RELEVANT_HOURS_SSC) && !string.IsNullOrEmpty(row[TOTAL_RELEVANT_HOURS_SSC]))
            {
                if (decimal.TryParse(row[TOTAL_RELEVANT_HOURS_SSC], out decimal totalRelevantHours))
                {
                    moqTable.TotalRelevantHours = totalRelevantHours;
                }
                else
                {
                    moqTable.ImportTypes.Add(MoqTableImportType.InvalidTotalRelevantHours);
                }
            }
            else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }
        }

        /// <summary>
        /// Populate the custom fields from the import file
        /// </summary>
        /// <param name="moqTable">Imported MOQ Table</param>
        /// <param name="row">row from the import file</param>
        /// <param name="customFields">MOQ Table custom fields</param>
        /// <param name="index">new ID index</param>
        private void ImportCustomFields(ImportedMoqTable moqTable, Dictionary<string, string> row, ICollection<CustomFieldDTO> customFields, ref int index)
        {
            if(customFields.Any())
            {
                foreach(CustomFieldDTO customField in customFields)
                {
                    string customFieldKey = ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName);

                    // Check to make sure required customFields are set
                    if (customField.CustomFieldRequired)
                    {
                        customFieldKey = ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName);
                        
                        {
                            moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
                        }
                    }

                    if (row.ContainsKey(customFieldKey) && !string.IsNullOrWhiteSpace(row[customFieldKey]))
                    {
                        string cfValue = row[customFieldKey];

                        moqTable.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                        {
                            Id = index--,
                            CustomFieldID = customField.Id,
                            CustomFieldValueID = index--,
                            Updateable = UpdateType.Upsert,
                            ContainerID = index--,
                            IsOpenEnded = true,
                            OpenEndedValue = cfValue.Trim(),
                            UpdateDate = DateTime.Now
                        });
                    }

                }
            }
        }

        /// <summary>
        /// Check if the imported fiscal week string is valid (ww/yyyy)
        /// </summary>
        /// <param name="fiscalWeekString">Fiscal week string to validate</param>
        /// <param name="weekValue">returned week value</param>
        /// <param name="yearValue">returned year value</param>
        /// <returns>True if fiscal week string was in the valid format</returns>
        private bool IsFiscalWeekValid(string fiscalWeekString, out int weekValue, out int yearValue)
        {
            string cleanedFiscalWeekString = fiscalWeekString.Replace("FW", string.Empty).Trim();
            string[] fiscalWeekValues = cleanedFiscalWeekString.Split('/');

            bool validWeek = int.TryParse(fiscalWeekValues[0], out weekValue);
            bool validYear = int.TryParse(fiscalWeekValues[1], out yearValue);

            return fiscalWeekValues.Length == 2 && validWeek && validYear && weekValue > 0 && weekValue <= 53 && yearValue > 1900;
        }
    }
}
