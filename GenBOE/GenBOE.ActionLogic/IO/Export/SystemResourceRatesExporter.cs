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
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for LaborRates-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SystemResourceRatesExporter
    {

        public static string ExportToExcelFile(string templateFileLocation, ICollection<ResourceRateDTO> inRates, IResourceDTODataLoader inResourceLoader)
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

            if (inResourceLoader == null)
            {
                throw new ArgumentNullException(nameof(inResourceLoader));
            }

            string toReturn = string.Empty;

			// Create collections of strings for each row in the export file
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();

            ICollection<ResourceDTO> resources = inResourceLoader.GetByIds(inRates.Select(x => x.ResourceID).Distinct().ToList());

            worksheet.AddRange(from rate in inRates
                               select new Collection<string>
                               {
                                   rate.ResourceRateID.ToString(),
                                   resources.First(x => x.Id == rate.ResourceID).ResourceName,
                                   rate.StartDate.Value.ToMonthString(),
                                   rate.EndDate.Value.ToMonthString(),
                                   rate.ResourceRate.ToString()
                               });

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }
    }
}
