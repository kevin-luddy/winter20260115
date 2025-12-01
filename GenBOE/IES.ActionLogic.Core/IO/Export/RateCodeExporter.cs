// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Reflection;
	using Common;
	using DataBridge.ModelViews;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;
	using IES.Common.Core;
	using IES.Common.Core.OfficeUtilities;

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
		/// Exports the rates to CSV file.  Expanded will expand the rates to their full size.
		/// </summary>
		/// <param name="templateFileLocation">The location of the Rate Excel file template</param>
		/// <param name="rates">The rates to export</param>
		/// <param name="expanded">The expanded rates</param>
		/// <returns>Filename for csv generated.</returns>
		public static string ExportToExcelFileWithYears(string templateFileLocation, ICollection<RateDetailModelView> rates, bool expanded)
        {
			// Create a new random file name in the specified directory
			string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

			// Create the document object in memory
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
			{
				WorkbookPart workbookPart = spreadsheet.WorkbookPart;
				if (workbookPart == null)
				{
					workbookPart = spreadsheet.AddWorkbookPart();
					workbookPart.Workbook = new Workbook();
				}

				// Remove existing sheets
				Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
				if (sheets != null)
				{
					sheets.RemoveAllChildren<Sheet>();
				}
				else
				{
					sheets = workbookPart.Workbook.AppendChild(new Sheets());
				}

				// Add worksheet
				WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
				worksheetPart.Worksheet = new Worksheet(new SheetData());

				Sheet sheet = new Sheet()
				{
					Id = workbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1,
					Name = "Rate Codes"
				};
				sheets.Append(sheet);

				PopulateRateCodesWithYears(rates, spreadsheet, expanded);
			}

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
			ExcelExportWorksheet worksheet = new();

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

			string sheetRange = ExcelUtilities.RedefineSheetDimensions(worksheetPart, (uint)rates.Rates.Count + 1U, 0);
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
			ExcelExportWorksheet optionsListWorksheet = new(ImportExportConstants.OPTIONS_LISTS);

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
			List<string> headerValues = new()
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
				List<string> optionValues = new()
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
			Dictionary<string, int> lengths = new()
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
			Dictionary<string, string> dataValidationReferences = new();

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

			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.GOVERNMENT_BURDEN_POOLS,
				ImportExportConstants.GOVT_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.COMMERCIAL_BURDEN_POOLS,
				ImportExportConstants.COMM_BURDEN_POOL_CELL_COLUMN_OFFSET + startDataColIndex, startDataRowIndex, endDataRowIndex);

			ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
		}

		/// <summary>
		/// Get csv headers
		/// </summary>
		/// <param name="startYear">starting year</param>
		/// <param name="endYear">ending year</param>
		/// <returns>Headers</returns>
		private static ICollection<string> GetCsvHeaders(int startYear, int endYear)
		{
			Collection<string> headers = new()
			{
				ImportExportConstants.RATE_CATEGORY_COLUMN_HEADER,
				ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER,
				ImportExportConstants.RATE_CODE_COLUMN_HEADER,
				
			};

			for (int i = startYear; i <= endYear; i++)
			{
				headers.Add(i.ToString());
			}

			return headers;
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

				data.GovernmentBurdenPoolId.HasValue ? rates.GovernmentBurdenPools.Single(x => x.Id == data.GovernmentBurdenPoolId).Label : string.Empty,
				data.CommercialBurdenPoolId.HasValue ? rates.CommercialBurdenPools.Single(x => x.Id == data.CommercialBurdenPoolId).Label : string.Empty
			};
		}

		/// <summary>
		/// Populates the Rate Codes with years
		/// </summary>
		/// <param name="rates">The rates to populate</param>
		/// <param name="spreadsheet">The spreadsheet to update</param>
		/// <param name="expanded">Whether to expand the ratecodes</param>
		/// <exception cref="NotImplementedException"></exception>
		private static void PopulateRateCodesWithYears(ICollection<RateDetailModelView> rates, SpreadsheetDocument spreadsheet, bool expanded)
		{
			// Create collections of strings for each row in the export file
			ExcelExportWorksheet worksheet = new();
			if (rates.Any())
			{
				int minYear = rates.Min(r => r.Values.First().Year);
				int maxYear = rates.Max(r => r.Values.Last().Year);

				ICollection<string> headers = GetCsvHeaders(minYear, maxYear);
				worksheet.Add(headers);

				foreach (RateDetailModelView data in rates)
				{
					AddRowAsStringCollection(worksheet, data, minYear, maxYear, expanded);
				}
			}

			// Export the data to the worksheet
			WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, string.Empty);
			ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 1);

			// save the worksheet
			worksheetPart.Worksheet.Save();
		}

		/// <summary>
		/// Adds the row as a string collection.  Expands as necessary
		/// </summary>
		/// <param name="worksheet">Excel worksheet</param>
		/// <param name="data">The data to add.</param>
		/// <param name="minYear">The min Year for values</param>
		/// <param name="maxYear">The max year for values</param>
		/// <param name="expanded">whether to expand the rate</param>
		private static void AddRowAsStringCollection(ExcelExportWorksheet worksheet, RateDetailModelView data, int minYear, int maxYear, bool expanded)
		{
			if (expanded && data.GenerateAdditionalDirectLaborRates)
			{
				if (data.DisclosureType == IES.Common.Core.Enums.DisclosureType.OneLMX)
				{
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription11, data.RateCode + "11", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription12, data.RateCode + "12", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription13, data.RateCode + "13", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription14, data.RateCode + "14", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription15, data.RateCode + "15", minYear, maxYear));

					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription21, data.RateCode + "21", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription22, data.RateCode + "22", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription23, data.RateCode + "23", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription24, data.RateCode + "24", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription25, data.RateCode + "25", minYear, maxYear));

					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription31, data.RateCode + "31", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription32, data.RateCode + "32", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription33, data.RateCode + "33", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription34, data.RateCode + "34", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription35, data.RateCode + "35", minYear, maxYear));

					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription41, data.RateCode + "41", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription42, data.RateCode + "42", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription43, data.RateCode + "43", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription44, data.RateCode + "44", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription45, data.RateCode + "45", minYear, maxYear));
					
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription51, data.RateCode + "51", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription52, data.RateCode + "52", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription53, data.RateCode + "53", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription54, data.RateCode + "54", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription55, data.RateCode + "55", minYear, maxYear));
					
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription61, data.RateCode + "61", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription62, data.RateCode + "62", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription63, data.RateCode + "63", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription64, data.RateCode + "64", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription65, data.RateCode + "65", minYear, maxYear));
					
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription71, data.RateCode + "71", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription72, data.RateCode + "72", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription73, data.RateCode + "73", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription74, data.RateCode + "74", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription75, data.RateCode + "75", minYear, maxYear));
					
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription81, data.RateCode + "81", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription82, data.RateCode + "82", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription83, data.RateCode + "83", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription84, data.RateCode + "84", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription85, data.RateCode + "85", minYear, maxYear));
					
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription91, data.RateCode + "91", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription92, data.RateCode + "92", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription93, data.RateCode + "93", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription94, data.RateCode + "94", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription95, data.RateCode + "95", minYear, maxYear));
				}
				else
				{
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription1, data.RateCode + "1", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription2, data.RateCode + "2", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription3, data.RateCode + "3", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription4, data.RateCode + "4", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription5, data.RateCode + "5", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription6, data.RateCode + "6", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription7, data.RateCode + "7", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription8, data.RateCode + "8", minYear, maxYear));
					AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.RateDescription9, data.RateCode + "9", minYear, maxYear));


				}
			}
			else
			{
				AddRowIfNotEmpty(worksheet, GetRowAsStringCollection(data.Values, data.RateCategoryDescription, data.Description, data.RateCode, minYear, maxYear));
			}
		}

		/// <summary>
		/// Null check for empty row
		/// </summary>
		/// <param name="worksheet">Worksheet</param>
		/// <param name="collection">row to add to worksheet</param>
		private static void AddRowIfNotEmpty(ExcelExportWorksheet worksheet, Collection<string> collection)
		{
			if (collection != null)
			{
				worksheet.Add(collection);
			}
		}

		/// <summary>
		/// Gets the row as a string collection
		/// </summary>
		/// <param name="values">Year values</param>
		/// <param name="category">rate category</param>
		/// <param name="description">rate description</param>
		/// <param name="rateCode">rate code</param>
		/// <param name="minYear">min year</param>
		/// <param name="maxYear">max year</param>
		/// <returns></returns>
		private static Collection<string> GetRowAsStringCollection(ICollection<RateYearModelView> values, string category, string description, string rateCode,  int minYear, int maxYear)
		{
			if (string.IsNullOrWhiteSpace(description))
			{
				return null;
			}

			Collection<string> row = new Collection<string>()
			{
				category,
				description,
				rateCode
			};

			for (int i = minYear; i <= maxYear; i++)
			{
				decimal? value = values.FirstOrDefault(v => v.Year == i)?.Value;
				row.Add(value?.ToString() ?? string.Empty);
			}

			return row;
		}
#pragma warning restore CA1505
	}
}