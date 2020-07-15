// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using GenBOE.Objects;

    /// <summary>
    /// Exports Travel Unit Costs to an Excel Spreadsheet.
    /// </summary>
    public interface ITravelUnitCostExporter
    {
        /// <summary>
        /// Exports to excel file.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>A string location of an excel file that has travel unit costs.</returns>
        string ExportToExcelFile(string templateFileLocation, FullWorkspace workspace);
    }
}
