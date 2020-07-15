// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Performing Orgs-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PerformingOrgsExporter
    {
        /// <summary>
        /// Exports default Performing Orgs to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Performing Orgs Excel file template</param>
        /// <param name="performingOrgs">The collection of Performing Orgs to export</param>
        /// <returns>Path to the exported Performing Orgs file</returns>
        public static string ExportToExcelFile(string templateFileLocation, Collection<PerformingOrgDTO> performingOrgs)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (performingOrgs == null)
            {
                throw new ArgumentNullException(nameof(performingOrgs));
            }

            string toReturn = string.Empty;

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            worksheet.AddRange(from performingOrg in performingOrgs
                               select new Collection<string>
                               {
                                   performingOrg.PerformingOrgName,
                                   performingOrg.PerformingOrgDesc
                               });

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
