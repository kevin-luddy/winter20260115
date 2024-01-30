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
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for System Offload Rates-specific Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SystemOffloadRatesExporterRMS
    {
        /// <summary>
        /// Exports default Offload Rates to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Offload Rates Excel file template</param>
        /// <param name="offloadRates">The offload rates.</param>
        /// <param name="systemPerformingOrgs">The system performing orgs.</param>
        /// <param name="systemResources">The system resources.</param>
        /// <returns>Path to the exported Offload Rates file</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<OffloadRatesDTO> offloadRates,
            ICollection<PerformingOrgDTO> systemPerformingOrgs, ICollection<ResourceDTO> systemResources)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (ReferenceEquals(offloadRates, null))
            {
                throw new ArgumentNullException(nameof(offloadRates));
            }

            // Create a new random file name in the specified directory
            string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            // Create the document object in memory
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
            {
                PopulateOptionsList(spreadsheet, systemPerformingOrgs, systemResources);
                PopulateRates(offloadRates, spreadsheet);
            }

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Populates the rates.
        /// </summary>
        /// <param name="offloadRates">The offload rates.</param>
        /// <param name="spreadsheet">The spreadsheet.</param>
        private static void PopulateRates(ICollection<OffloadRatesDTO> offloadRates, SpreadsheetDocument spreadsheet)
        {
			// Create collections of strings for each row in the export file
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();

            if (offloadRates.Any())
            {
                worksheet.AddRange(from rate in offloadRates
                                   select new Collection<string>
                                {
                                    rate.Id.ToString(),
                                    rate.Year.ToString(),
                                    rate.Resource,
                                    rate.PerformingOrg,
                                    rate.SubResource,
                                    rate.Percent.ToString(),
                                    rate.HourlyRate.ToString()
                                });
            }

            // Export the Labor data to row 2 of the worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, "Rates");
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 2); // start on the 2nd row so we do not overwrite the headers

            AddDataValidation(worksheetPart, offloadRates.Count);

            string sheetRange = ExcelUtilities.RedefineSheetDimensions(worksheetPart, ((uint)offloadRates.Count) + 1U, 0);
            ExcelUtilities.SetIgnoredErrors(worksheetPart.Worksheet, sheetRange);

            // save the worksheet
            worksheetPart.Worksheet.Save();
        }

        /// <summary>
        /// Populates the options list worksheet.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="systemPerformingOrgs">All performing orgs.</param>
        /// <param name="systemResources">All resource types.</param>
        private static void PopulateOptionsList(SpreadsheetDocument spreadsheet, ICollection<PerformingOrgDTO> systemPerformingOrgs, 
            ICollection<ResourceDTO> systemResources)
        {
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

            ICollection<ResourceDTO> laborResources = systemResources.Where(r => r.ElementOfCost == ElementOfCostType.LMLabor).ToList();
            ICollection<ResourceDTO> subResources = systemResources.Where(r => r.ElementOfCost == ElementOfCostType.Sub).ToList();

            // Options List Column Headers
            List<string> headerValues = new List<string>();
            headerValues.Add(ImportExportConstants.RESOURCE_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.PERF_ORG_COLUMN_HEADER);
            headerValues.Add(ImportExportConstants.SUB_RESOURCE_COLUMN_HEADER);

            int maxRows = Math.Max(subResources.Count, Math.Max(systemPerformingOrgs.Count, laborResources.Count));

            optionsListWorksheet.Add(headerValues);   // Add option list header row

            // Add option value rows
            for (int i = 0; i < maxRows; i++)
            {
                List<string> optionValues = new List<string>();
                optionValues.Add(laborResources.Count > i ? laborResources.ElementAt(i).ResourceName : string.Empty);
                optionValues.Add(systemPerformingOrgs.Count > i ? systemPerformingOrgs.ElementAt(i).PerformingOrgName : string.Empty);
                optionValues.Add(subResources.Count > i ? subResources.ElementAt(i).ResourceName : string.Empty);

                optionsListWorksheet.Add(optionValues);
            }

            // Export the Options List headers and data to row 1 of the "Options List" worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsListWorksheet.WorksheetName);
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsListWorksheet, 1);

            // Adjust existing Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { ImportExportConstants.RESOURCES, laborResources.Count },
                { ImportExportConstants.PERFORGS, systemPerformingOrgs.Count },
                { ImportExportConstants.SUB_RESOURCES, subResources.Count },
            };
            ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);
        }

        /// <summary>
        /// Adds the data validation dropdowns to the excel spreadsheet.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        /// <param name="ratesCount">The rates count.</param>
        private static void AddDataValidation(WorksheetPart worksheetPart, int ratesCount)
        {
            // Create the data validation dropdowns for custom fields
            uint startDataRowIndex = 2;
            int startDataColIndex = 1;
            uint endDataRowIndex = (uint)(ratesCount + 11); // all of the data rows and ten extra
            
            // Adjust the spreadoffset by the customfields and multi columns 
            Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();
            
            // Add data validation references for Resource, Performing Org, and Spread Curve columns
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCES,
                ImportExportConstants.RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.PERFORGS,
                ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.SUB_RESOURCES,
                ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET + 1 + startDataColIndex, startDataRowIndex, endDataRowIndex);

            ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
        }
    }
}
