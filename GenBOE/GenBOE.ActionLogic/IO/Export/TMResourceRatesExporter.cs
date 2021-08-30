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
    /// Responsible for T&amp;M LaborRates-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TMResourceRatesExporter
    {

        public static string ExportToExcelFile(string templateFileLocation, ICollection<ResourceRateDTO> inRates, ICollection<ResourceDTO> inResources, ICollection<ResourceDTO> inOtherResources)
        {
            // Check inputs2
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (inRates == null)
            {
                throw new ArgumentNullException(nameof(inRates));
            }

            if (inResources == null)
            {
                throw new ArgumentNullException(nameof(inResources));
            }

            string toReturn = string.Empty;

            //get only rates that are actually exportable (non Mapped too).
            inRates = (from exportableRates in inRates where exportableRates.EndDate.HasValue && exportableRates.StartDate.HasValue && exportableRates.ResourceRate.HasValue select exportableRates).ToList();

            var optionsListWorksheet = new ExcelExportWorksheet("OptionsList");
            optionsListWorksheet.AddRange(from rate in inResources
                select new Collection<string>
                {
                    rate.ResourceName.ToString()
                });


            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet("TMResourceRates");

            if (inRates.Count > 0)
            {
                worksheet.AddRange(from rate in inRates
                    select new Collection<string>
                    {
                        rate.ResourceRateID.ToString(),
                        inOtherResources.First(c => c.Id==rate.ResourceID).ResourceName,
                        rate.StartDate.Value.ToString("MM/yyyy"),
                        rate.EndDate.Value.ToString("MM/yyyy"),
                        rate.ResourceRate.ToString()
                    });
            }

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, optionsListWorksheet, worksheet);

            // Adjust Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { "Resource_Names", inResources.Count },
            };

            ExcelExporter.AdjustDefinedNames(toReturn, lengths);

            // Return the file path
            return toReturn;
        }
    }
}
