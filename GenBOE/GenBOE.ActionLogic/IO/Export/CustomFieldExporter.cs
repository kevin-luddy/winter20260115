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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Custom Field-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CustomFieldExporter
    {
        /// <summary>
        /// Exports Custom Field Values to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Custom Field Excel file template</param>
        /// <param name="customFieldValues">The collection of Custom Field Values to export</param>
        /// <returns>Path to the exported Custom Fields file</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<CustomFieldValueDTO> customFieldValues)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (customFieldValues == null)
            {
                throw new ArgumentNullException(nameof(customFieldValues));
            }

            string toReturn = string.Empty;

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            worksheet.AddRange(from customFieldValue in customFieldValues
                                select new Collection<string>
                                {
                                    customFieldValue.CustomFieldValueName,
                                    customFieldValue.CustomFieldValueDescription
                                });

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
