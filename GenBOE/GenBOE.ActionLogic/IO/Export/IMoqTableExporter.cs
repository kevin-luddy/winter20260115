// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Objects;

    public interface IMoqTableExporter
    {
        /// <summary>
        /// MOQ Table export excel file path
        /// </summary>
        string MOQ_TABLE_EXCEL_MAP_PATH { get; }

        /// <summary>
        /// Exports MOQ Table data to Excel
        /// </summary>
        /// <param name="templateFileLocation">Location of the excel template file</param>
        /// <param name="moqTables">MOQ Table data</param>
        /// <param name="ws">Workspace</param>
        /// <returns>Name of the generated report file</returns>
        string ExportToExcelFile(string templateFileLocation, ICollection<MoqTableData> moqTables, FullWorkspace ws);
    }
}
