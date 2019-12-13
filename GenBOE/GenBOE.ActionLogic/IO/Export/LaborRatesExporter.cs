using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GenBOE.Common.IO.Utilities;
using GenBOE.ModelView.DTO;
using System.Collections.Generic;

namespace GenBOE.Common.IO.Export
{
    /// <summary>
    /// Responsible for LaborRates-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class LaborRatesExporter
    {

        public static string ExportToExcelFile(string templateFileLocation, IEnumerable<LaborResourceRateDTO> inRates)
        {
            // Check inputs
            if (templateFileLocation == null)
                throw new ArgumentNullException("templateFileLocation");
            if (inRates == null)
                throw new ArgumentNullException("inRates");

            string toReturn = string.Empty;

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            worksheet.AddRange(from rate in inRates
                               select new Collection<string>
                               {
                                   rate.ResourceRateID.ToString(),
                                   rate.ResourceID.ToString(),
                                   rate.StartDate.ToString("MM/yyyy"),
                                   rate.EndDate.ToString("MM/yyyy"),
                                   rate.ResourceRate.ToString()
                               });

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
