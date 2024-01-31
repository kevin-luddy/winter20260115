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
    /// Responsible for System Escalation Rates-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SystemEscalationRatesExporterRMS
    {
        /// <summary>
        /// Exports default Escalation Rates to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Escalation Rates Excel file template</param>
        /// <param name="escalationRates">The collection of Escalation Rates to export</param>
        /// <returns>Path to the exported Escalation Rates file</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<EscalationRatesDTO> escalationRates)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (ReferenceEquals(escalationRates, null))
            {
                throw new ArgumentNullException(nameof(escalationRates));
            }

            string toReturn = string.Empty;

			// Create collections of strings for each row in the export file
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();

            if (escalationRates.Any())
            {
                worksheet.AddRange(from rate in escalationRates
                                   select new Collection<string>
                                {
                                    rate.EscalationRateID.ToString(),
                                    rate.Year.ToString(),
                                    (100m * rate.DevEscalation).ToString() // show as a percentage by multiplying by 100
                                });
            }
            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
