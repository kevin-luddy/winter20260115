// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.DataBridge.Common;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Resource-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ResourcesExporter
    {
        /// <summary>
        /// Exports default resources to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Resources Excel file template</param>
        /// <param name="resources">The collection of resources to export</param>
        /// <returns>Path to the exported resources file</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<ResourceDTO> resources, ICommonDataMapper inCommonMapper )
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (inCommonMapper == null)
            {
                throw new ArgumentNullException(nameof(inCommonMapper));
            }
            
            string toReturn = string.Empty;

            // Create collections of strings for each row in the export file
            var optionsListWorksheet = new ExcelExportWorksheet("Options Lists");

            Collection<RateTypeModelView> allRateTypes = inCommonMapper.GetRateTypes();
            Collection<ElementOfCostTypeModelView> allElementOfCosts = inCommonMapper.GetElementOfCostTypes();           

            int maxRows = Math.Max(allRateTypes.Count, allElementOfCosts.Count);

            for (int i = 0; i < maxRows; i++)
            {
                optionsListWorksheet.Add(new Collection<string>
                    {
                        allRateTypes.ElementAtOrDefault(i) != null ? allRateTypes[i].RateTypeName : string.Empty,
                        allElementOfCosts.ElementAtOrDefault(i) != null ? allElementOfCosts[i].ElementOfCostName : string.Empty
                    });
            }

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            if (resources != null)
            {
                worksheet.AddRange(from resource in resources
                                   select new Collection<string>
                                {
                                    resource.ResourceName,
                                    resource.ResourceDesc,
                                    resource.SegRegion,
                                    resource.LaborType,
                                    resource.RateType.GetDescription(),
                                    allElementOfCosts.Where(x => x.ElementOfCostId == (int)resource.ElementOfCost).Select(x => x.ElementOfCostName).FirstOrDefault()
                                });
            }
            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, optionsListWorksheet ,worksheet);

            // Adjust Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { "RateType", allRateTypes.Count },
                { "ElementOfCost", allElementOfCosts.Count }
            };

            ExcelExporter.AdjustDefinedNames(toReturn, lengths);

            // Return the file path
            return toReturn;
        }

        public static string ExportTemplate(string templateFileLocation, ICommonDataMapper commonMapper)
        {
            return ExportToExcelFile(templateFileLocation, null ,commonMapper);
        }
    }
}
