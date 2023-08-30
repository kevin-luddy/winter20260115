// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common;
    using DataBridge.ModelViews;
    using DocumentFormat.OpenXml.Packaging;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Responsible for Rate Code Excel export.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class RateCodeExporter
    {
        /// <summary>
        /// Exports Rate Codes to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Rate Code Excel file template</param>
        /// <param name="rates">The Rate Codes and related data.</param>
        /// <returns>Path to the exported Rate Code file</returns>
        public static string ExportToExcelFile(string templateFileLocation, RateGridModelView rates)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (rates == null)
            {
                throw new ArgumentNullException(nameof(rates));
            }

            // Create a new random file name in the specified directory
            string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            // Create the document object in memory
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
            {
                PopulateOptionsList(spreadsheet, rates);
                PopulateRateCodes(rates, spreadsheet);
            }

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Populates the Rate Codes.
        /// </summary>
        /// <param name="rates">The Rate Codes and related data.</param>
        /// <param name="spreadsheet">The spreadsheet.</param>
        private static void PopulateRateCodes(RateGridModelView rates, SpreadsheetDocument spreadsheet)
        {
            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            Collection<string> headers = new Collection<string>()
            {
                ImportExportConstants.RATE_CATEGORY_COLUMN_HEADER,
                ImportExportConstants.RATE_CODE_COLUMN_HEADER,
                ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER,
                ImportExportConstants.LINKED_SECTION_COLUMN_HEADER,
                ImportExportConstants.RESOURCE_TYPE_COLUMN_HEADER,
                ImportExportConstants.RATE_TYPE_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION1_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS1_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION2_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS2_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION3_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS3_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION4_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS4_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION5_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS5_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION6_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS6_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_DESCRIPTION7_COLUMN_HEADER,
                ImportExportConstants.PRO_PRICER_RESOURCE_CLASS7_COLUMN_HEADER,
                ImportExportConstants.GOVERNMENT_BURDEN_POOL_COLUMN_HEADER,
                ImportExportConstants.COMMERCIAL_BURDEN_POOL_COLUMN_HEADER
            };

            worksheet.Add(headers);

            if (rates.Rates.Any())
            {
                foreach (RateDetailModelView data in rates.Rates)
                {
                    Collection<string> row = new Collection<string>
                    {
                        data.RateCategory.GetDescription(),
                        data.RateCode,
                        data.Description,
                        data.Section.HasValue ? rates.Sections.Single(x => x.Id == data.Section).Label : string.Empty,
                        data.ResourceType.GetDescription(),
                        data.RateType.GetDescription(),
                        data.RateDescription,
                        data.ResourceClassId.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId).Label : string.Empty,
                        data.RateDescription1,
                        data.ResourceClassId1.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId1).Label : string.Empty,
                        data.RateDescription2,
                        data.ResourceClassId2.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId2).Label : string.Empty,
                        data.RateDescription3,
                        data.ResourceClassId3.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId3).Label : string.Empty,
                        data.RateDescription4,
                        data.ResourceClassId4.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId4).Label : string.Empty,
                        data.RateDescription5,
                        data.ResourceClassId5.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId5).Label : string.Empty,
                        data.RateDescription6,
                        data.ResourceClassId6.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId6).Label : string.Empty,
                        data.RateDescription7,
                        data.ResourceClassId7.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId7).Label : string.Empty,
                        data.RateDescription8,
                        data.ResourceClassId8.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId8).Label : string.Empty,
                        data.GovernmentBurdenPoolId.HasValue ? rates.GovernmentBurdenPools.Single(x => x.Id == data.GovernmentBurdenPoolId).Label : string.Empty,
                        data.CommercialBurdenPoolId.HasValue ? rates.CommercialBurdenPools.Single(x => x.Id == data.CommercialBurdenPoolId).Label : string.Empty
                    };

                    worksheet.Add(row);
                }
            }

            // Export the data to the worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, ImportExportConstants.RATE_CODES);
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 1);

            AddDataValidation(worksheetPart, rates.Rates.Count);

            string sheetRange = ExcelUtilities.RedefineSheetDimensions(worksheetPart, ((uint)rates.Rates.Count) + 1U, 0);
            ExcelUtilities.SetIgnoredErrors(worksheetPart.Worksheet, sheetRange);

            // save the worksheet
            worksheetPart.Worksheet.Save();
        }

        /// <summary>
        /// Populates the options list worksheet.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet.</param>
        /// <param name="rates">The Rate Codes and related data.</param>
        private static void PopulateOptionsList(SpreadsheetDocument spreadsheet, RateGridModelView rates)
        {
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

            string[] rateCategoryOptions = rates.RateCategories.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();
            string[] sectionOptions = rates.Sections.Where(x => x.Id > 0).Select(x => x.Label).ToArray();
            string[] resourceTypeOptions = rates.ResourceTypes.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();
            string[] rateTypeOptions = rates.RateTypes.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();
            string[] resourceClassOptions = rates.ResourceClasses.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();
            string[] govtBurdenPoolOptions = rates.GovernmentBurdenPools.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();
            string[] commBurdenPoolOptions = rates.CommercialBurdenPools.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();

            // Options List Column Headers
            List<string> headerValues = new List<string>
            {
                ImportExportConstants.CATEGORIES,
                ImportExportConstants.SECTIONS,
                ImportExportConstants.RESOURCE_TYPES,
                ImportExportConstants.RATE_TYPES,
                ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.GOVERNMENT_BURDEN_POOLS,
                ImportExportConstants.COMMERCIAL_BURDEN_POOLS
            };

            int maxRows = Math.Max(rateCategoryOptions.Length, Math.Max(sectionOptions.Length, Math.Max(resourceTypeOptions.Length, Math.Max(rateTypeOptions.Length, Math.Max(resourceClassOptions.Length, Math.Max(govtBurdenPoolOptions.Length, commBurdenPoolOptions.Length))))));
            optionsListWorksheet.Add(headerValues);   // Add option list header row

            // Add option value rows
            for (int i = 0; i < maxRows; i++)
            {
                List<string> optionValues = new List<string>
                {
                    rateCategoryOptions.Length > i ? rateCategoryOptions[i] : string.Empty,
                    sectionOptions.Length > i ? sectionOptions[i] : string.Empty,
                    resourceTypeOptions.Length > i ? resourceTypeOptions[i] : string.Empty,
                    rateTypeOptions.Length > i ? rateTypeOptions[i] : string.Empty,
                    resourceClassOptions.Length > i ? resourceClassOptions[i] : string.Empty,
                    govtBurdenPoolOptions.Length > i ? govtBurdenPoolOptions[i] : string.Empty,
                    commBurdenPoolOptions.Length > i ? commBurdenPoolOptions[i] : string.Empty
                };

                optionsListWorksheet.Add(optionValues);
            }

            // Export the Options List headers and data to row 1 of the "Options List" worksheet
            WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsListWorksheet.WorksheetName);
            ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsListWorksheet, 1);

            // Adjust existing Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { ImportExportConstants.CATEGORIES, rateCategoryOptions.Length },
                { ImportExportConstants.SECTIONS, sectionOptions.Length },
                { ImportExportConstants.RESOURCE_TYPES, resourceTypeOptions.Length },
                { ImportExportConstants.RATE_TYPES, rateTypeOptions.Length },
                { ImportExportConstants.RESOURCE_CLASSES, resourceClassOptions.Length },
                { ImportExportConstants.GOVERNMENT_BURDEN_POOLS, govtBurdenPoolOptions.Length },
                { ImportExportConstants.COMMERCIAL_BURDEN_POOLS, commBurdenPoolOptions.Length }
            };

            ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);
        }

        /// <summary>
        /// Adds the data validation dropdowns to the excel spreadsheet.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part.</param>
        /// <param name="rateCodeResourcesCount">The Rate Code resources count.</param>
        private static void AddDataValidation(WorksheetPart worksheetPart, int rateCodeResourcesCount)
        {
            // Create the data validation dropdowns for custom fields
            uint startDataRowIndex = 2;
            int startDataColIndex = 0;
            uint endDataRowIndex = (uint)(rateCodeResourcesCount + 11); // all of the data rows and ten extra

            // Adjust the spread offset by the custom fields and multi columns 
            Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();

            // Add data validation references for Lookup columns
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CATEGORIES,
                ImportExportConstants.RATE_CATEGORY_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.SECTIONS,
                ImportExportConstants.SECTION_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_TYPES,
                ImportExportConstants.RESOURCE_TYPE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RATE_TYPES,
                ImportExportConstants.RATE_TYPE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS1_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS2_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS3_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS4_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS5_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS6_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
                ImportExportConstants.RESOURCE_CLASS7_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.GOVERNMENT_BURDEN_POOLS,
                ImportExportConstants.GOVT_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
            ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.COMMERCIAL_BURDEN_POOLS,
                ImportExportConstants.COMM_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

            ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
        }
    }
}