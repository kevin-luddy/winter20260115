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
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    public class MoqTableExporter: IMoqTableExporter
    {
        /// <summary>
        /// MOQ Table export excel file path
        /// </summary>
        public virtual string MOQ_TABLE_EXCEL_MAP_PATH => "MOQTableSSC.xlsx";

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTableExporter()
        { }

        /// <summary>
        /// Exports MOQ Table data to Excel
        /// </summary>
        /// <param name="templateFileLocation">Location of the excel template file</param>
        /// <param name="moqTables">MOQ Table data</param>
        /// <param name="ws">Workspace</param>
        /// <returns>Name of the generated report file</returns>
        public string ExportToExcelFile(string templateFileLocation, ICollection<MoqTableData> moqTables, FullWorkspace ws)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));

            string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            this.DuplicateCustomFieldColumns(toReturn, ws);
            ExcelExportWorksheet worksheet = this.GetMoqTableData(moqTables, ws);
            toReturn = ExcelExporter.ExportToExcelFile(toReturn, false, new Collection<ExcelExportWorksheet>() { worksheet }, new int?[] { null }, FullObjectHelper.ShowEquivalentPersonsOption && ws.IsUsingEquivalentPerson);

            return toReturn;
        }

        /// <summary>
        /// Duplicates the custom field columns.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="exportInputs">The export inputs.</param>
        private void DuplicateCustomFieldColumns(string templateFileLocation, FullWorkspace ws)
        {
            string[] moqTableCustomFieldNames = ws.CustomFields.Where(c => c.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay).Select(c => c.CustomFieldName).ToArray();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(templateFileLocation, true))
            {
                if (moqTableCustomFieldNames.Any())
                {
                    ExcelUtilities.DuplicateColumn(document, "MOQ Tables", "MOQ Table Custom Field", moqTableCustomFieldNames);
                }
                else
                {
                    ExcelUtilities.RemoveColumn(document, "MOQ Tables", "MOQ Table Custom Field");
                }
            }
        }

        /// <summary>
        /// Get the MOQ Table data for the export worksheet
        /// </summary>
        /// <param name="moqTables">MOQ Tables</param>
        /// <param name="ws">Workspace</param>
        /// <returns>MOQ Table data in an excel worksheet</returns>
        private ExcelExportWorksheet GetMoqTableData(ICollection<MoqTableData> moqTables, FullWorkspace ws)
        {
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet("MOQ Tables");

            foreach(MoqTableData table in moqTables)
            {
                toReturn.Add(GetMOQTableDataRow(table, ws));
            }

            return toReturn;
        }

        /// <summary>
        /// Get the MOQ Table Data row data
        /// </summary>
        /// <param name="table">MOQ Table</param>
        /// <param name="ws">Workspace</param>
        /// <returns>MOQ Table Data row for the given data</returns>
        protected virtual IList<string> GetMOQTableDataRow(MoqTableData table, FullWorkspace ws)
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));
            _ = ws ?? throw new ArgumentNullException(nameof(ws));

            IList<string> toReturn = new List<string>()
            {
                table.TableName,
                table.RepositoryName,
                table.QueryType,
                table.DateOfReport.ToShortDateString(),
                table.HistoricalProgramName,
                table.WbsElement,
                table.PoPStartString,
                table.PoPEndString,
                table.AdditionalQueryFilters,
                table.TotalRelevantHours.ToString()
            };

            foreach (CustomFieldDTO customField in ws.CustomFields.Where(x => x.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay))
            {
                CustomFieldValueContainer customFieldValue = table.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id);
                toReturn.Add(customFieldValue != null ? customFieldValue.OpenEndedValue : string.Empty);
            }

            return toReturn;
        }
    }
}
