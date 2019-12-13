// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using GenBOE.Objects;

    /// <summary>
    /// Exports Extended Travel Cost data to an Excel File.
    /// </summary>
    public interface ITravelExtendedCostExporter
    {
        /// <summary>
        /// Exports to excel file.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="inWorkspace">The workspace.</param>
        /// <returns>A string location of an excel file that has travel extended costs.</returns>
        string ExportToExcelFile(string templateFileLocation, FullWorkspace inWorkspace);
    }
}
