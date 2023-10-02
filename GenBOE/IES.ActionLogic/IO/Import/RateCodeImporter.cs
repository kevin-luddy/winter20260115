// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Import
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using Common;
	using DataBridge.ModelViews;
	using DocumentFormat.OpenXml.Packaging;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Responsible for RateCode-specific Excel import.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class RateCodeImporter
	{
		#region Constants

		/// <summary>
		/// Array of the columns that must be contained in the imported file.
		/// </summary>
		private static readonly string[] RequiredColumns = new string[]
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

		/// <summary>
		/// Array of the columns in the imported file that must contain values.
		/// </summary>
		private static readonly string[] RequiredValueColumns = new string[]
		{
			ImportExportConstants.RATE_CATEGORY_COLUMN_HEADER,
			ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER,
			ImportExportConstants.RATE_CODE_COLUMN_HEADER,
			ImportExportConstants.DISCLOSURE_TYPE_COLUMN_HEADER
		};

		/// <summary>
		/// Array of the columns in the imported file that must contain unique values.
		/// </summary>
		private static readonly string[] UniqueValueColumns = new string[] { };

		/// <summary>
		/// Array of the columns that must be parsed as text.
		/// </summary>
		private static readonly string[] TextOnlyColumns = new string[] { };

		/// <summary>
		/// The logger for the class.
		/// </summary>
		private static Logger logger = new Logger(typeof(RateCodeImporter));

		#endregion Constants

		#region Public Functions

		/// <summary>
		/// Returns a collection of Rate Detail ModelViews.
		/// </summary>
		/// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
		/// <param name="rates">The Rate Codes and related data.</param>
		/// <returns>Collection of new Rate Code ModelView objects.</returns>
		public static ICollection<RateDetailModelView> ImportFromExcelFile(Stream excelFileStream, RateGridModelView rates)
		{
			ICollection<Dictionary<string, string>> allRows = GetAllRows(excelFileStream);

			// Turn each row into a DTO object and return the collection
			return CreateDTOsToReturn(allRows, rates);
		}

		#endregion Public Functions

		#region Private Functions

		/// <summary>
		/// Gets all rows to process
		/// </summary>
		/// <param name="excelFileStream">File Stream</param>
		/// <returns>Dictionary containing imported rows</returns>
		private static ICollection<Dictionary<string, string>> GetAllRows(Stream excelFileStream)
		{
			ICollection<Dictionary<string, string>> allRows;
			try
			{
				// Open the document as read-only.
				using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
				{
					// Get a collection of all rows in the file, filtering out rows that only have data in
					// non import-related columns. Each row is represented as a Key/Value pair Dictionary object
					// in an enumerable collection

					List<string> requiredColumns = new List<string>(RequiredColumns);

					List<string> allColumns = new List<string>(RequiredColumns);

					allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns.ToArray(),
						allColumns.ToArray(), RequiredValueColumns, UniqueValueColumns, TextOnlyColumns);
				}
			}
			catch (FileFormatException)
			{
				logger.Error("Imported Rate Codes file was an incorrect format.");
				throw new NotExcelFileException();
			}
			catch (ColumnMissingException cme)
			{
				logger.Error("Imported Rate Codes file was missing a required column.");
				throw new ColumnMissingException("Import file was missing a required column: " + cme.Message);
			}
			catch (CellValueMissingException cvme)
			{
				logger.Error("Imported Rate Codes file was missing a required cell value.");
				throw new CellValueMissingException("Import file was missing a required cell value. Check the following column: " + cvme.Message);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw;
			}

			return allRows;
		}

		/// <summary>
		/// Converts a collection of Key/Value pair Dictionary objects into a collection of DTOs.
		/// </summary>
		/// <param name="allRows">Collection of Key/Value pair Dictionary objects representing imported rows.</param>
		/// <param name="rates">The Rate Codes and related data.</param>
		/// <returns>A collection of DTOs representing the newly imported values</returns>
		private static ICollection<RateDetailModelView> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows, RateGridModelView rates)
		{
			// Create the collection to return
			List<RateDetailModelView> toReturn = new List<RateDetailModelView>();

			// For each Dictionary object (representing imported row data)
			foreach (Dictionary<string, string> row in allRows)
			{
				RateDetailModelView rateCodeMapRow = new RateDetailModelView();

				// Add the new Rate Code to the collection to be returned
				toReturn.Add(rateCodeMapRow);

				// Set the Rate Code Properties
				rateCodeMapRow.RevisionId = rates.SelectedRevisionId.Value;
				rateCodeMapRow.RateCategory = (RateCategory)GetIdFromCell(row, ImportExportConstants.RATE_CATEGORY_COLUMN_HEADER, rates.RateCategories);
				rateCodeMapRow.RateCode = row.ContainsKey(ImportExportConstants.RATE_CODE_COLUMN_HEADER) ? row[ImportExportConstants.RATE_CODE_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.Description = row.ContainsKey(ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER) ? row[ImportExportConstants.RATE_DESCRIPTION_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.Section = GetIdFromCell(row, ImportExportConstants.LINKED_SECTION_COLUMN_HEADER, rates.Sections);
				int? id = GetNullableIdFromCell(row, ImportExportConstants.RESOURCE_TYPE_COLUMN_HEADER, rates.ResourceTypes);
				if (id.HasValue)
				{
					rateCodeMapRow.ResourceType = (DirectRateMappingResourceType)id;
				}

				id = GetNullableIdFromCell(row, ImportExportConstants.RATE_TYPE_COLUMN_HEADER, rates.RateTypes);
				if (id.HasValue)
				{
					rateCodeMapRow.RateType = (RateType)id;
				}

				id = GetNullableIdFromCell(row, ImportExportConstants.DISCLOSURE_TYPE_COLUMN_HEADER, rates.DisclosureTypes);
				if (id.HasValue)
				{
					rateCodeMapRow.DisclosureType = (DisclosureType)id;
				}

				rateCodeMapRow.RateDescription = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription1 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION1_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION1_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId1 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS1_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription2 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION2_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION2_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId2 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS2_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription3 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION3_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION3_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId3 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS3_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription4 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION4_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION4_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId4 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS4_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription5 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION5_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION5_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId5 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS5_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription6 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION6_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION6_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId6 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS6_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription7 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION7_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION7_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId7 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS7_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription8 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION8_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION8_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId8 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS8_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription9 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION9_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION9_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId9 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS9_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.GovernmentBurdenPoolId = GetNullableIdFromCell(row, ImportExportConstants.GOVERNMENT_BURDEN_POOL_COLUMN_HEADER, rates.GovernmentBurdenPools);
				rateCodeMapRow.CommercialBurdenPoolId = GetNullableIdFromCell(row, ImportExportConstants.COMMERCIAL_BURDEN_POOL_COLUMN_HEADER, rates.CommercialBurdenPools);
				rateCodeMapRow.Values = new Collection<RateYearModelView>();
				rateCodeMapRow.GenerateAdditionalDirectLaborRates =
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription1) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription2) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription3) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription4) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription5) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription6) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription7) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription8) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription9);
			}

			// Return the collection of new DTOs
			return toReturn;
		}

		/// <summary>
		/// Get select list Id value from cell.
		/// </summary>
		/// <param name="row">Row contents</param>
		/// <param name="columnHeader">Column header name</param>
		/// <param name="allOptions">Collection of all options for the select list.</param>
		/// <returns>Id of selected option</returns>
		private static int GetIdFromCell(Dictionary<string, string> row, string columnHeader, ICollection<OptionModelView> allOptions)
		{
			if (row.ContainsKey(columnHeader))
			{
				string cellValue = row[columnHeader].Trim();
				if (string.IsNullOrWhiteSpace(cellValue))
				{
					return 0;
				}

				OptionModelView option = allOptions.FirstOrDefault(x => x.Label.Trim().Equals(cellValue));
				if (option == null)
				{
					throw new GenValidationException(columnHeader + " is invalid");
				}

				return option.Id;
			}

			return 0;
		}

		/// <summary>
		/// Get nullable select list Id value from cell.
		/// </summary>
		/// <param name="row">Row contents</param>
		/// <param name="columnHeader">Column header name</param>
		/// <param name="allOptions">Collection of all options for the select list.</param>
		/// <returns>Id of selected option</returns>
		private static int? GetNullableIdFromCell(Dictionary<string, string> row, string columnHeader, ICollection<OptionModelView> allOptions)
		{
			if (row.ContainsKey(columnHeader))
			{
				string cellValue = row[columnHeader].Trim();
				if (string.IsNullOrWhiteSpace(cellValue))
				{
					return null;
				}

				OptionModelView option = allOptions.FirstOrDefault(x => x.Label.Trim().Equals(cellValue));
				if (option == null)
				{
					throw new GenValidationException(columnHeader + " is invalid");
				}

				return option.Id;
			}

			return null;
		}

		#endregion Private Functions
	}
}