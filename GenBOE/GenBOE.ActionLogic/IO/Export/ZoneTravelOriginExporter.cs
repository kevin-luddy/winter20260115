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
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Zone Travel Origins-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ZoneTravelOriginExporter
    {
        /// <summary>
        /// Exports default resources to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Resources Excel file template</param>
        /// <param name="origins">The collection of resources to export</param>
        /// <returns>Path to the exported resources file</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<MSTZoneTravelOriginModelView> origins)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (ReferenceEquals(origins, null))
            {
                throw new ArgumentNullException(nameof(origins));
            }

            string toReturn = string.Empty;

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            if (origins.Any())
            {
                worksheet.AddRange(from origin in origins
                                   select new Collection<string>
                                {
                                    origin.OriginID.ToString(),
                                    origin.Origin,
                                    origin.Site,
                                    origin.ResourcePRZ1 ?? string.Empty,
                                    origin.ResourceTRZ1 ?? string.Empty,
                                    origin.ResourcePRZ2 ?? string.Empty,
                                    origin.ResourceTRZ2 ?? string.Empty,
                                    origin.ResourcePRZ3 ?? string.Empty,
                                    origin.ResourceTRZ3 ?? string.Empty,
                                    origin.ResourcePRZ4 ?? string.Empty,
                                    origin.ResourceTRZ4 ?? string.Empty,
                                    origin.ResourcePRZ5 ?? string.Empty,
                                    origin.ResourceTRZ5 ?? string.Empty,
                                    origin.ResourcePRZ6 ?? string.Empty,
                                    origin.ResourceTRZ6 ?? string.Empty
                                });
            }
            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
