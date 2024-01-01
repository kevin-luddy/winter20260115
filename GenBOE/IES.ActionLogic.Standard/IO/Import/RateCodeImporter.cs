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
	using IES.Standard;
	using IES.Standard;
	using IES.Standard.Exceptions;
	using IES.Standard.OfficeUtilities;

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

			#region 1LMX Required Columns

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
		private static ILogger logger = new Logger(typeof(RateCodeImporter));

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
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
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

				#region 1LMX Rows

				// Level 1
				rateCodeMapRow.RateDescription11 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION11_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION11_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId11 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS11_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription12 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION12_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION12_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId12 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS12_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription13 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION13_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION13_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId13 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS13_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription14 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION14_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION14_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId14 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS14_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription15 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION15_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION15_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId15 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS15_COLUMN_HEADER, rates.ResourceClasses);

				// Level 2
				rateCodeMapRow.RateDescription21 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION21_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION21_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId21 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS21_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription22 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION22_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION22_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId22 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS22_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription23 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION23_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION23_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId23 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS23_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription24 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION24_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION24_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId24 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS24_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription25 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION25_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION25_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId25 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS25_COLUMN_HEADER, rates.ResourceClasses);

				// Level 3
				rateCodeMapRow.RateDescription31 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION31_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION31_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId31 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS31_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription32 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION32_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION32_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId32 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS32_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription33 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION33_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION33_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId33 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS33_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription34 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION34_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION34_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId34 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS34_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription35 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION35_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION35_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId35 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS35_COLUMN_HEADER, rates.ResourceClasses);

				// Level 4
				rateCodeMapRow.RateDescription41 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION41_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION41_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId41 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS41_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription42 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION42_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION42_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId42 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS42_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription43 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION43_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION43_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId43 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS43_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription44 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION44_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION44_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId44 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS44_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription45 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION45_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION45_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId45 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS45_COLUMN_HEADER, rates.ResourceClasses);

				// Level 5
				rateCodeMapRow.RateDescription51 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION51_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION51_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId51 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS51_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription52 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION52_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION52_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId52 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS52_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription53 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION53_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION53_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId53 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS53_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription54 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION54_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION54_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId54 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS54_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription55 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION55_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION55_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId55 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS55_COLUMN_HEADER, rates.ResourceClasses);

				// Level 6
				rateCodeMapRow.RateDescription61 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION61_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION61_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId61 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS61_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription62 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION62_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION62_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId62 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS62_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription63 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION63_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION63_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId63 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS63_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription64 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION64_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION64_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId64 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS64_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription65 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION65_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION65_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId65 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS65_COLUMN_HEADER, rates.ResourceClasses);

				// Level 7
				rateCodeMapRow.RateDescription71 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION71_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION71_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId71 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS71_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription72 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION72_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION72_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId72 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS72_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription73 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION73_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION73_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId73 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS73_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription74 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION74_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION74_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId74 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS74_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription75 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION75_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION75_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId75 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS75_COLUMN_HEADER, rates.ResourceClasses);

				// Level 8
				rateCodeMapRow.RateDescription81 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION81_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION81_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId81 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS81_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription82 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION82_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION82_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId82 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS82_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription83 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION83_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION83_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId83 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS83_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription84 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION84_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION84_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId84 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS84_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription85 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION85_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION85_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId85 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS85_COLUMN_HEADER, rates.ResourceClasses);

				// Level 9
				rateCodeMapRow.RateDescription91 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION91_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION91_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId91 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS91_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription92 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION92_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION92_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId92 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS92_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription93 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION93_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION93_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId93 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS93_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription94 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION94_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION94_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId94 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS94_COLUMN_HEADER, rates.ResourceClasses);
				rateCodeMapRow.RateDescription95 = row.ContainsKey(ImportExportConstants.PRO_PRICER_DESCRIPTION95_COLUMN_HEADER) ? row[ImportExportConstants.PRO_PRICER_DESCRIPTION95_COLUMN_HEADER] : string.Empty;
				rateCodeMapRow.ResourceClassId95 = GetNullableIdFromCell(row, ImportExportConstants.PRO_PRICER_RESOURCE_CLASS95_COLUMN_HEADER, rates.ResourceClasses);

				#endregion

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
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription9) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription11) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription12) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription13) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription14) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription15) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription21) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription22) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription23) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription24) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription25) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription31) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription32) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription33) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription34) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription35) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription41) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription42) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription43) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription44) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription45) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription51) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription52) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription53) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription54) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription55) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription61) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription62) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription63) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription64) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription65) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription71) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription72) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription73) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription74) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription75) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription81) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription82) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription83) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription84) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription85) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription91) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription92) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription93) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription94) ||
					!string.IsNullOrWhiteSpace(rateCodeMapRow.RateDescription95);
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