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
	using IES.Core;
	using IES.Core.OfficeUtilities;

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

			Collection<string> headers = GetHeaders();

			worksheet.Add(headers);

			if (rates.Rates.Any())
			{
				foreach (RateDetailModelView data in rates.Rates)
				{
					Collection<string> row = GetRowAsStringCollection(data, rates);

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
			string[] disclosureTypeOptions = rates.DisclosureTypes.Where(x => x.Id > 0).OrderBy(x => x.Label).Select(x => x.Label).ToArray();

			// Options List Column Headers
			// Really the Defined Names for the dropdown lists
			List<string> headerValues = new List<string>
			{
				ImportExportConstants.CATEGORIES,
				ImportExportConstants.SECTIONS,
				ImportExportConstants.RESOURCE_TYPES,
				ImportExportConstants.RATE_TYPES,
				ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.GOVERNMENT_BURDEN_POOLS,
				ImportExportConstants.COMMERCIAL_BURDEN_POOLS,
				ImportExportConstants.DISCLOSURE_TYPES
			};

			int maxRows = Math.Max(rateCategoryOptions.Length, Math.Max(sectionOptions.Length, Math.Max(resourceTypeOptions.Length, Math.Max(rateTypeOptions.Length, Math.Max(disclosureTypeOptions.Length, Math.Max(resourceClassOptions.Length, Math.Max(govtBurdenPoolOptions.Length, commBurdenPoolOptions.Length)))))));
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
					commBurdenPoolOptions.Length > i ? commBurdenPoolOptions[i] : string.Empty,
					disclosureTypeOptions.Length > i ? disclosureTypeOptions[i] : string.Empty
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
				{ ImportExportConstants.COMMERCIAL_BURDEN_POOLS, commBurdenPoolOptions.Length },
				{ ImportExportConstants.DISCLOSURE_TYPES, disclosureTypeOptions.Length },
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
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.DISCLOSURE_TYPES,
			ImportExportConstants.DISCLOSURE_TYPE_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
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
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS8_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS9_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			#region 1LMX Validation References

			// Level 1
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS11_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS12_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS13_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS14_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS15_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 2
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS21_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS22_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS23_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS24_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS25_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 3
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS31_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS32_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS33_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS34_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS35_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 4
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS41_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS42_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS43_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS44_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS45_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 5
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS51_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS52_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS53_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS54_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS55_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 6
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS61_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS62_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS63_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS64_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS65_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 7
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS71_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS72_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS73_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS74_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS75_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 8
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS81_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS82_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS83_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS84_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS85_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			// Level 9
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS91_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS92_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS93_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS94_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCE_CLASSES,
				ImportExportConstants.RESOURCE_CLASS95_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			#endregion

			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.GOVERNMENT_BURDEN_POOLS,
				ImportExportConstants.GOVT_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.COMMERCIAL_BURDEN_POOLS,
				ImportExportConstants.COMM_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
		}

		private static Collection<string> GetHeaders()
		{
			return new Collection<string>()
			{
				ImportExportConstants.RATE_CATEGORY_COLUMN_HEADER,
				ImportExportConstants.RATE_CODE_COLUMN_HEADER,
				ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER,
				ImportExportConstants.LINKED_SECTION_COLUMN_HEADER,
				ImportExportConstants.RESOURCE_TYPE_COLUMN_HEADER,
				ImportExportConstants.RATE_TYPE_COLUMN_HEADER,
				ImportExportConstants.DISCLOSURE_TYPE_COLUMN_HEADER,
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
				ImportExportConstants.PRO_PRICER_DESCRIPTION8_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS8_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION9_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS9_COLUMN_HEADER,

				#region 1LMX Headers

				// Level 1
				ImportExportConstants.PRO_PRICER_DESCRIPTION11_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS11_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION12_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS12_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION13_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS13_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION14_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS14_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION15_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS15_COLUMN_HEADER,

				// Level 2
				ImportExportConstants.PRO_PRICER_DESCRIPTION21_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS21_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION22_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS22_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION23_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS23_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION24_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS24_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION25_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS25_COLUMN_HEADER,

				// Level 3
				ImportExportConstants.PRO_PRICER_DESCRIPTION31_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS31_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION32_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS32_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION33_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS33_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION34_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS34_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION35_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS35_COLUMN_HEADER,

				// Level 4
				ImportExportConstants.PRO_PRICER_DESCRIPTION41_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS41_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION42_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS42_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION43_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS43_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION44_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS44_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION45_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS45_COLUMN_HEADER,

				// Level 5
				ImportExportConstants.PRO_PRICER_DESCRIPTION51_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS51_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION52_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS52_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION53_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS53_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION54_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS54_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION55_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS55_COLUMN_HEADER,

				// Level 6
				ImportExportConstants.PRO_PRICER_DESCRIPTION61_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS61_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION62_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS62_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION63_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS63_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION64_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS64_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION65_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS65_COLUMN_HEADER,

				// Level 7
				ImportExportConstants.PRO_PRICER_DESCRIPTION71_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS71_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION72_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS72_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION73_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS73_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION74_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS74_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION75_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS75_COLUMN_HEADER,

				// Level 8
				ImportExportConstants.PRO_PRICER_DESCRIPTION81_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS81_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION82_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS82_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION83_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS83_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION84_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS84_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION85_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS85_COLUMN_HEADER,

				// Level 9
				ImportExportConstants.PRO_PRICER_DESCRIPTION91_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS91_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION92_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS92_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION93_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS93_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION94_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS94_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_DESCRIPTION95_COLUMN_HEADER,
				ImportExportConstants.PRO_PRICER_RESOURCE_CLASS95_COLUMN_HEADER,

				#endregion

				ImportExportConstants.GOVERNMENT_BURDEN_POOL_COLUMN_HEADER,
				ImportExportConstants.COMMERCIAL_BURDEN_POOL_COLUMN_HEADER
			};
		}

		#pragma warning disable CA1505
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private static Collection<string> GetRowAsStringCollection(RateDetailModelView data, RateGridModelView rates)
		{
			return new Collection<string>
			{
				data.RateCategory.GetDescription(),
				data.RateCode,
				data.Description,
				data.Section.HasValue ? rates.Sections.Single(x => x.Id == data.Section).Label : string.Empty,
				data.ResourceType.GetDescription(),
				data.RateType.GetDescription(),
				data.DisclosureType.GetDescription(),
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
				data.RateDescription9,
				data.ResourceClassId9.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId9).Label : string.Empty,

				#region 1LMX Writes

				// Level 1
				data.RateDescription11,
				data.ResourceClassId11.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId11).Label : string.Empty,
				data.RateDescription12,
				data.ResourceClassId12.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId12).Label : string.Empty,
				data.RateDescription13,
				data.ResourceClassId13.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId13).Label : string.Empty,
				data.RateDescription14,
				data.ResourceClassId14.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId14).Label : string.Empty,
				data.RateDescription15,
				data.ResourceClassId15.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId15).Label : string.Empty,

				// Level 2
				data.RateDescription21,
				data.ResourceClassId21.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId21).Label : string.Empty,
				data.RateDescription22,
				data.ResourceClassId22.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId22).Label : string.Empty,
				data.RateDescription23,
				data.ResourceClassId23.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId23).Label : string.Empty,
				data.RateDescription24,
				data.ResourceClassId24.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId24).Label : string.Empty,
				data.RateDescription25,
				data.ResourceClassId25.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId25).Label : string.Empty,

				// Level 3
				data.RateDescription31,
				data.ResourceClassId31.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId31).Label : string.Empty,
				data.RateDescription32,
				data.ResourceClassId32.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId32).Label : string.Empty,
				data.RateDescription33,
				data.ResourceClassId33.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId33).Label : string.Empty,
				data.RateDescription34,
				data.ResourceClassId34.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId34).Label : string.Empty,
				data.RateDescription35,
				data.ResourceClassId35.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId35).Label : string.Empty,

				// Level 4
				data.RateDescription41,
				data.ResourceClassId41.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId41).Label : string.Empty,
				data.RateDescription42,
				data.ResourceClassId42.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId42).Label : string.Empty,
				data.RateDescription43,
				data.ResourceClassId43.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId43).Label : string.Empty,
				data.RateDescription44,
				data.ResourceClassId44.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId44).Label : string.Empty,
				data.RateDescription45,
				data.ResourceClassId45.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId45).Label : string.Empty,

				// Level 5
				data.RateDescription51,
				data.ResourceClassId51.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId51).Label : string.Empty,
				data.RateDescription52,
				data.ResourceClassId52.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId52).Label : string.Empty,
				data.RateDescription53,
				data.ResourceClassId53.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId53).Label : string.Empty,
				data.RateDescription54,
				data.ResourceClassId54.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId54).Label : string.Empty,
				data.RateDescription55,
				data.ResourceClassId55.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId55).Label : string.Empty,

				// Level 6
				data.RateDescription61,
				data.ResourceClassId61.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId61).Label : string.Empty,
				data.RateDescription62,
				data.ResourceClassId62.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId62).Label : string.Empty,
				data.RateDescription63,
				data.ResourceClassId63.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId63).Label : string.Empty,
				data.RateDescription64,
				data.ResourceClassId64.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId64).Label : string.Empty,
				data.RateDescription65,
				data.ResourceClassId65.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId65).Label : string.Empty,

				// Level 7
				data.RateDescription71,
				data.ResourceClassId71.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId71).Label : string.Empty,
				data.RateDescription72,
				data.ResourceClassId72.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId72).Label : string.Empty,
				data.RateDescription73,
				data.ResourceClassId73.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId73).Label : string.Empty,
				data.RateDescription74,
				data.ResourceClassId74.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId74).Label : string.Empty,
				data.RateDescription75,
				data.ResourceClassId75.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId75).Label : string.Empty,

				// Level 8
				data.RateDescription81,
				data.ResourceClassId81.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId81).Label : string.Empty,
				data.RateDescription82,
				data.ResourceClassId82.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId82).Label : string.Empty,
				data.RateDescription83,
				data.ResourceClassId83.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId83).Label : string.Empty,
				data.RateDescription84,
				data.ResourceClassId84.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId84).Label : string.Empty,
				data.RateDescription85,
				data.ResourceClassId85.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId85).Label : string.Empty,

				// Level 9
				data.RateDescription91,
				data.ResourceClassId91.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId91).Label : string.Empty,
				data.RateDescription92,
				data.ResourceClassId92.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId92).Label : string.Empty,
				data.RateDescription93,
				data.ResourceClassId93.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId93).Label : string.Empty,
				data.RateDescription94,
				data.ResourceClassId94.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId94).Label : string.Empty,
				data.RateDescription95,
				data.ResourceClassId95.HasValue ? rates.ResourceClasses.Single(x => x.Id == data.ResourceClassId95).Label : string.Empty,

				#endregion

				data.GovernmentBurdenPoolId.HasValue ? rates.GovernmentBurdenPools.Single(x => x.Id == data.GovernmentBurdenPoolId).Label : string.Empty,
				data.CommercialBurdenPoolId.HasValue ? rates.CommercialBurdenPools.Single(x => x.Id == data.CommercialBurdenPoolId).Label : string.Empty
			};
		}
		#pragma warning restore CA1505
	}
}