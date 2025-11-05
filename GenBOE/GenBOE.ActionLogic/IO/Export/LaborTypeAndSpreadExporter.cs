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
	using DocumentFormat.OpenXml.Spreadsheet;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	[ExcludeFromCodeCoverage]
	public static class LaborTypeAndSpreadExporter
	{
		private static Logger log = new Logger(typeof(LaborTypeAndSpreadExporter));

		private const string IMPORT_TAB = "Labor Types";
		private const string RESOURCE_COST_HEADER = "Cost";
		private const UInt32 StyleIndexGeneral = 1;
		private const UInt32 StyleIndexCurrency = 5;


		#region Public Functions

		/// <summary>
		/// Exports the Labor Types and Spread to an excel file.
		/// </summary>
		/// <param name="templateFileLocation">The template file location.</param>
		/// <param name="inResourceLoader">The resource loader.</param>
		/// <param name="inCommonMapper">The common mapper.</param>
		/// <param name="inWorkspace">The workspace.</param>
		/// <param name="inTaskElement">The task element.</param>
		/// <param name="boeId">The BOE id.</param>
		/// <param name="isTemplate">Whether the export is for just the template or includes the data.</param>
		/// <returns>The location of the filled in template file.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// inResourceLoader or inCommonMapper or inWorkspace or inTaskElement</exception>
		public static string ExportToExcelFile(string templateFileLocation,
			ResourceDTODataLoader inResourceLoader,
			ICommonDataMapper inCommonMapper,
			FullWorkspace inWorkspace,
			BoeTaskElementDTO inTaskElement,
			int boeId,
			bool isTemplate)
		{
			if (inResourceLoader == null)
			{
				throw new ArgumentNullException(nameof(inResourceLoader));
			}

			if (inCommonMapper == null)
			{
				throw new ArgumentNullException(nameof(inCommonMapper));
			}

			if (inWorkspace == null)
			{
				throw new ArgumentNullException(nameof(inWorkspace));
			}

			if (inTaskElement == null)
			{
				throw new ArgumentNullException(nameof(inTaskElement));
			}

			ICollection<ResourceDTO> originalResources = inResourceLoader.GetByListIdAndElementOfCost(
				inWorkspace.ResourceListID,
				new Collection<ElementOfCostType>() { ElementOfCostType.LMLabor, ElementOfCostType.IWTA, ElementOfCostType.Sub, ElementOfCostType.ODC, ElementOfCostType.Travel, ElementOfCostType.Materials });

			List<CustomFieldDTO> workspaceCustomFields = inWorkspace.CustomFields.Where(cf => cf.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay).ToList();
			ICollection<int> customFieldIds = workspaceCustomFields.Select(cf => cf.Id).ToList();
			ICollection<CustomFieldValueDTO> workspaceCustomFieldValues = inWorkspace.CustomFieldValues.Where(cfv => customFieldIds.Contains(cfv.CustomFieldID)).ToList();
			bool isMulti = inWorkspace.Boes.First(b => b.Id == boeId).IsMultiClinWbs;
			bool isOffload = inWorkspace.ProjectMapType == ProjectMapType.StandardWithOffload;

			IReadOnlyCollection<PerformingOrgDTO> allPerformingOrgs = inWorkspace.PerformingOrgsForWsList;
			Collection<SpreadCurveModelView> allCurves = inCommonMapper.getSpreadCurve();
			ICollection<ResourceDTO> allResourceTypes = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResources, false, inWorkspace.Shortname);
			ICollection<ResourceDTO> allBusinessResourceCodeTypes = new List<ResourceDTO>();

			if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
			{
				allBusinessResourceCodeTypes = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResources, true, inWorkspace.Shortname);
			}

			// Create a new random file name in the specified directory
			string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

			// Duplicate the columns for custom fields
			PopulateDynamicColumns(toReturn, workspaceCustomFields);

			// remove multi columns if not needed
			RemoveMultiColumns(toReturn, isMulti);

			// remove Offload column if not needed
			RemoveOffloadColumn(toReturn, isOffload);

			// Create the document object in memory
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
			{
				PopulateOptionsList(spreadsheet, allPerformingOrgs, allCurves, allResourceTypes, workspaceCustomFields, workspaceCustomFieldValues, inWorkspace.WbsElementsNoMultiWbs, inWorkspace.ClinsNoMultiClin, inWorkspace, allBusinessResourceCodeTypes);
				PopulateLaborTypeAndSpread(spreadsheet, inWorkspace, inTaskElement, allPerformingOrgs, allCurves, allResourceTypes, workspaceCustomFields, workspaceCustomFieldValues, isMulti, isTemplate, isOffload, allBusinessResourceCodeTypes);
			}

			FormatLaborExport(toReturn);
			return toReturn;
		}

		#endregion Public Functions

		#region Private Functions

		/// <summary>
		/// Cleans up LaborSpreadsExport format, so cost is currency and the month values are general style so imports dont break.
		/// </summary>
		/// <param name="inTemplateFileLocation">Template file location</param>
		private static void FormatLaborExport(string inTemplateFileLocation)
		{
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(inTemplateFileLocation, true))
			{
				string columnName = ExcelUtilities.GetColumnNameFromHeaderString(spreadsheet, IMPORT_TAB, RESOURCE_COST_HEADER);
				int columnIndex = ExcelUtilities.GetColumnIndexFromColumnName(columnName);
				WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, IMPORT_TAB);
				if (worksheetPart != null)
				{
					List<Cell> cellsToFormat = worksheetPart.Worksheet.Descendants<Cell>().Where(c => ExcelUtilities.ParseColumnName(c.CellReference).Equals(columnName, StringComparison.CurrentCultureIgnoreCase)).ToList();
					foreach (Cell cell in cellsToFormat.Skip(1))
					{
						cell.StyleIndex = StyleIndexCurrency;
					}
					int numMonthsToFormat = worksheetPart.Worksheet.Descendants<Row>().First().ChildElements.Count();
					for (int ndx = columnIndex + 1; ndx <= numMonthsToFormat; ndx++)
					{
						List<Cell> monthCellsToFormat = worksheetPart.Worksheet.Descendants<Cell>().Where(c => ExcelUtilities.ParseColumnName(c.CellReference).Equals(ExcelUtilities.GetColumnNameFromColumnIndex(ndx), StringComparison.CurrentCultureIgnoreCase)).ToList();
						foreach (Cell cell in monthCellsToFormat.Skip(1))
						{
							cell.StyleIndex = StyleIndexGeneral;
						}
					}
				}
			}
		}

		/// <summary>
		/// Populates the dynamic columns.
		/// </summary>
		/// <param name="inTemplateFileLocation">The location of the Excel template.</param>
		/// <param name="workspaceCustomFields">The labor resource custom fields in the workspace.</param>
		private static void PopulateDynamicColumns(string inTemplateFileLocation, ICollection<CustomFieldDTO> workspaceCustomFields)
		{
			string[] customFieldColumnHeaders = workspaceCustomFields.Select(cf => cf.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(cf.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(cf.CustomFieldName)).ToArray();

			// Insert the Resource Custom Field Columns
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(inTemplateFileLocation, true))
			{
				if (customFieldColumnHeaders.Length > 0)
				{
					ExcelUtilities.DuplicateColumn(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, ImportExportConstants.RESOURCE_CUSTOM_FIELD_HEADER, customFieldColumnHeaders.ToArray());
				}
				else
				{
					ExcelUtilities.RemoveColumn(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, ImportExportConstants.RESOURCE_CUSTOM_FIELD_HEADER);
				}
			}
		}

		/// <summary>
		/// Removes the offload column if it is not needed.
		/// </summary>
		/// <param name="inTemplateFileLocation">The template file location.</param>
		/// <param name="showOffload">if set to <c>true</c> [show offload].</param>
		private static void RemoveOffloadColumn(string inTemplateFileLocation, bool showOffload)
		{
			if (!showOffload)
			{
				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(inTemplateFileLocation, true))
				{
					ExcelUtilities.RemoveColumn(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, ImportExportConstants.OFFLOAD_COLUMN_HEADER);
				}
			}
		}

		/// <summary>
		/// Removes the columns if the BOE's not multi
		/// </summary>
		/// <param name="inTemplateFileLocation">The Document</param>
		/// <param name="isMultiBOE">BOE Multi</param>
		private static void RemoveMultiColumns(string inTemplateFileLocation, bool isMultiBOE)
		{
			if (!isMultiBOE)
			{
				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(inTemplateFileLocation, true))
				{

					ExcelUtilities.RemoveColumn(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER);
					ExcelUtilities.RemoveColumn(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB, LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER);
				}
			}
		}

		/// <summary>
		/// Populates the labor type and spread worksheet.
		/// </summary>
		/// <param name="spreadsheet">The spreadsheet.</param>
		/// <param name="inWorkspace">The workspace.</param>
		/// <param name="inTaskElement">The task element.</param>
		/// <param name="allPerformingOrgs">All performing orgs.</param>
		/// <param name="allCurves">All curves.</param>
		/// <param name="allResourceTypes">All resource types.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="workspaceCustomFieldValues">The workspace custom field values.</param>
		/// <param name="isMulti">Whether the BOE is a Multi-Clin/WBS BOE or not.</param>
		/// <param name="isTemplate">Whether the export is for just the template or includes the data.</param>
		/// <param name="isOffload">True if we should show the Offload column; otherwise false.</param>
		/// <param name="allBusinessResourceCodes">All Business Resource Code Types</param>
		private static void PopulateLaborTypeAndSpread(SpreadsheetDocument spreadsheet, FullWorkspace inWorkspace, BoeTaskElementDTO inTaskElement, IReadOnlyCollection<PerformingOrgDTO> allPerformingOrgs,
			ICollection<SpreadCurveModelView> allCurves, ICollection<ResourceDTO> allResourceTypes, ICollection<CustomFieldDTO> workspaceCustomFields,
			ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, bool isMulti, bool isTemplate, bool isOffload,
			ICollection<ResourceDTO> allBusinessResourceCodes)
		{
			// determination complete range of spread dates across all resources (i.e. the total number of spread month columns needed)
			DateTime spreadStartDate = inTaskElement.StartDate.Value;
			DateTime spreadEndDate = inTaskElement.EndDate.Value;
			if (isTemplate)
			{
				// We have the start and end dates, now null out the task element so we do not output any data
				inTaskElement = null;
			}

			Collection<ResourceTypeDto> laborResources = inTaskElement != null ? inTaskElement.taskElementLabors : new Collection<ResourceTypeDto>();

			if (laborResources.Any(r => r.StartDate.HasValue && r.EndDate.HasValue))
			{
				spreadStartDate = laborResources.Where(r => r.StartDate.HasValue).Select(r => r.StartDate.Value).Min();
				spreadEndDate = laborResources.Where(r => r.EndDate.HasValue).Select(r => r.EndDate.Value).Max();
			}

			// create a lookup table to determine column index for each spread month
			int totalSpreadMonths = 0;
			List<string> spreadMonthColumnHeaders = new List<string>();
			Dictionary<DateTime, int> spreadDateColumnIndices = new Dictionary<DateTime, int>();
			for (DateTime dt = spreadStartDate; dt <= spreadEndDate; dt = dt.AddMonths(1))
			{
				spreadDateColumnIndices.Add(dt, totalSpreadMonths++);
				spreadMonthColumnHeaders.Add(dt.ToString("MM/yyyy"));
			}

			ExcelExportWorksheet laborTypeWorksheet = new ExcelExportWorksheet(LaborTypeAndSpreadImporter.IMPORT_TAB);

			// Get Task Resource custom fields and values
			Dictionary<int, ICollection<KeyValuePair<int, int>>> laborTypeCustomFieldValueIdMappings = inWorkspace.LaborTypesMappingWithCustomFieldsValuesAndContainerIds;
			IDictionary<CustomFieldValueDTO, CustomFieldDTO> allTaskResourcesCustomFields = CreateCustomFieldDictionary(workspaceCustomFields, workspaceCustomFieldValues);
			int multiColumnsCount = 2;
			if (!isMulti)
			{
				multiColumnsCount = 0;
			}

			int offloadColumnCount = isOffload ? 1 : 0;

			// Populate Header data
			List<string> headerRow = CreateHeaderRow(workspaceCustomFields, spreadMonthColumnHeaders, isMulti, FullObjectHelper.ShowEquivalentPersonsOption && inWorkspace.IsUsingEquivalentPerson, isOffload, inWorkspace.Shortname);
			laborTypeWorksheet.Add(headerRow);

			if (laborResources.Any())
			{
				// Populate Labor data
				foreach (ResourceTypeDto laborType in laborResources)
				{
					List<string> row = CreateLaborRow(inWorkspace, inTaskElement, allPerformingOrgs, allCurves, allResourceTypes, allBusinessResourceCodes, spreadDateColumnIndices, laborTypeCustomFieldValueIdMappings,
						allTaskResourcesCustomFields, laborType, workspaceCustomFields, isMulti, isOffload);

					laborTypeWorksheet.Add(row);
				}
			}
			else
			{
				// enter a blank row so that Excel does not cause problems opening the file when the rows are not in the correct order
				List<string> emptyRow = Enumerable.Repeat(string.Empty, headerRow.Count).ToList();
				laborTypeWorksheet.Add(emptyRow);
			}

			// Export the Labor data to row 2 of the worksheet
			WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, LaborTypeAndSpreadImporter.IMPORT_TAB);
			ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, laborTypeWorksheet, 1);

			AddDataValidation(worksheetPart, workspaceCustomFields, laborResources.Count, isMulti, isOffload, inWorkspace.Shortname);

			string sheetRange = ExcelUtilities.RedefineSheetDimensions(worksheetPart, ((uint)laborResources.Count) + 1U, workspaceCustomFields.Count + multiColumnsCount + offloadColumnCount + spreadDateColumnIndices.Count - 1);
			ExcelUtilities.SetIgnoredErrors(worksheetPart.Worksheet, sheetRange);

			// save the worksheet
			worksheetPart.Worksheet.Save();
		}

		/// <summary>
		/// Adds the data validation dropdowns to the excel spreadsheet.
		/// </summary>
		/// <param name="worksheetPart">The worksheet part.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="laborResourcesCount">The labor resources count.</param>
		/// <param name="isMulti">Bool to note if the BOE is a Multi Clin/WBS BOE</param>
		/// <param name="isOffload">True if we should show the Offload column; otherwise false.</param>
		/// <param name="workspaceShortname">Workspace short name</param>
		private static void AddDataValidation(WorksheetPart worksheetPart, ICollection<CustomFieldDTO> workspaceCustomFields, int laborResourcesCount, bool isMulti, bool isOffload, string workspaceShortname)
		{
			// Create the data validation dropdowns for custom fields
			uint startDataRowIndex = 2;
			uint endDataRowIndex = (uint)(laborResourcesCount + ImportExportConstants.LABOR_TYPES_DROPDOWN_VALIDATION_OFFSET); // all of the data rows and 100 extra for offset.
			int customFieldOffset = 1;
			int performingOrgCellColumnOffset = ImportExportConstants.RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET;
			
			if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
			{
				// add a column before performing org
				performingOrgCellColumnOffset++;
			}

			//adjust the spreadoffset by the customfields and multi columns 
			int spreadOffset = workspaceCustomFields.Count;
			Dictionary<string, string> dataValidationReferences = new Dictionary<string, string>();
			foreach (CustomFieldDTO customFieldForLabel in workspaceCustomFields)
			{
				// add data validation for the custom field value cell if not open ended
				if (!customFieldForLabel.IsOpenEnded)
				{
					ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customFieldForLabel.Id,
						performingOrgCellColumnOffset + customFieldOffset, startDataRowIndex, endDataRowIndex);
				}

				customFieldOffset++;
			}
			if (isMulti)
			{
				ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.WBSS, performingOrgCellColumnOffset + customFieldOffset, startDataRowIndex, endDataRowIndex);
				customFieldOffset++;
				ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.CLINS, performingOrgCellColumnOffset + customFieldOffset, startDataRowIndex, endDataRowIndex);
				customFieldOffset++;
				spreadOffset += 2;
			}

			// Setting offset here as the predefined names will not allow feature flag to work properly
			int columnOffset = ImportExportConstants.RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET;

			// Add data validation references for Resource, Performing Org, and Spread Curve columns
			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.RESOURCES,
				columnOffset++, startDataRowIndex, endDataRowIndex);

			if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
			{
				ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.BUSINESS_RESOURCE_CODES,
					columnOffset++, startDataRowIndex, endDataRowIndex);
			}

			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.PERFORGS,
				columnOffset++, startDataRowIndex, endDataRowIndex);

			// We increment by 2 to get to Spread Curve Column
			columnOffset += 2;

			ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.SPREAD_CURVES,
				(columnOffset++) + spreadOffset, startDataRowIndex, endDataRowIndex);

			if (isOffload)
			{
				// No need to increment columnOffset
				int offloadOffset = columnOffset + spreadOffset + 4;
				ExcelExporter.AddCellReferenceToDataValidationDictionary(dataValidationReferences, ImportExportConstants.OFFLOAD, offloadOffset, startDataRowIndex, endDataRowIndex);
			}

			ExcelExporter.AddDataValidations(dataValidationReferences, worksheetPart);
		}

		/// <summary>
		/// Creates the header row.
		/// </summary>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="spreadMonthColumnHeaders">The spread month column headers.</param>
		/// <param name="isMulti">if set to <c>true</c> [is multi].</param>
		/// <param name="isUsingEquivalentPerson">True if we should be using EP instead of Hours.</param>
		/// <param name="isOffload">True if we are showing offload column.</param>
		/// <param name="workspaceShortname">Workspace short name</param>
		/// <returns>
		/// List of strings representing cells in the header row.
		/// </returns>
		private static List<string> CreateHeaderRow(ICollection<CustomFieldDTO> workspaceCustomFields, List<string> spreadMonthColumnHeaders, bool isMulti, bool isUsingEquivalentPerson, bool isOffload, string workspaceShortname)
		{
			List<string> headerRow = new List<string>() { LaborTypeAndSpreadImporter.LABOR_TYPE_ID_COL, ImportExportConstants.RESOURCE_COLUMN_HEADER };

			if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
			{
				headerRow.Add(ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER);
			}

			headerRow.Add(LaborTypeAndSpreadImporter.PERFORMING_ORG_COL);

			foreach (CustomFieldDTO customField in workspaceCustomFields)
			{
				headerRow.Add(customField.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName));
			}
			if (isMulti)
			{
				headerRow.Add(LaborTypeAndSpreadImporter.RESOURCE_WBS_HEADER);
				headerRow.Add(LaborTypeAndSpreadImporter.RESOURCE_CLIN_HEADER);
			}

			headerRow.Add(ImportExportConstants.START_DATE_COLUMN_HEADER);
			headerRow.Add(ImportExportConstants.END_DATE_COLUMN_HEADER);
			headerRow.Add(ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER);
			headerRow.Add(LaborTypeAndSpreadImporter.PERCENT_SPREAD_COL);

			if (isUsingEquivalentPerson)
			{
				headerRow.Add(LaborTypeAndSpreadImporter.EP_SPREAD_COL);
			}
			else
			{
				headerRow.Add(LaborTypeAndSpreadImporter.HOURS_SPREAD_COL);
			}
			headerRow.Add(LaborTypeAndSpreadImporter.COST_COL);

			if (isOffload)
			{
				headerRow.Add(ImportExportConstants.OFFLOAD_COLUMN_HEADER);
			}

			headerRow.AddRange(spreadMonthColumnHeaders);

			return headerRow;
		}

		/// <summary>
		/// Creates the labor row.
		/// </summary>
		/// <param name="inWorkspace">The workspace.</param>
		/// <param name="inTaskElement">The task element.</param>
		/// <param name="allPerformingOrgs">All performing organizations.</param>
		/// <param name="allCurves">All curves.</param>
		/// <param name="allResourceTypes">All resource types.</param>
		/// <param name="spreadDateColumnIndices">The spread date column indices.</param>
		/// <param name="laborTypeCustomFieldValueIdMappings">The mapping of values for a labor type and a custom field value.</param>
		/// <param name="customFieldDictionary">The custom field value dictionary.</param>
		/// <param name="laborType">The labor type resource.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="isMulti">if set to <c>true</c> [is multi].</param>
		/// <param name="isOffload">True if we should show the Offload column; otherwise false.</param>
		/// <returns></returns>
		private static List<string> CreateLaborRow(FullWorkspace inWorkspace, BoeTaskElementDTO inTaskElement, IReadOnlyCollection<PerformingOrgDTO> allPerformingOrgs,
			ICollection<SpreadCurveModelView> allCurves, ICollection<ResourceDTO> allResourceTypes, ICollection<ResourceDTO> allBusinessResourceTypes, Dictionary<DateTime, int> spreadDateColumnIndices,
			Dictionary<int, ICollection<KeyValuePair<int, int>>> laborTypeCustomFieldValueIdMappings, IDictionary<CustomFieldValueDTO, CustomFieldDTO> customFieldDictionary,
			ResourceTypeDto laborType, ICollection<CustomFieldDTO> workspaceCustomFields, bool isMulti, bool isOffload)
		{
			ResourceDTO thisResource = (from resources in allResourceTypes where laborType.ResourceID.HasValue && resources.Id == laborType.ResourceID select resources).FirstOrDefault();
			ResourceDTO thisBusinessResourceCode = (from businessResourceCodes in allBusinessResourceTypes where laborType.BusinessResourceCodeID.HasValue && businessResourceCodes.Id == laborType.BusinessResourceCodeID select businessResourceCodes).FirstOrDefault();
			PerformingOrgDTO thisPerfOrg = (from perOrgs in allPerformingOrgs where laborType.PerformingOrgID.HasValue && perOrgs.Id == laborType.PerformingOrgID select perOrgs).FirstOrDefault();
			SpreadCurveModelView thisSpread = (from curves in allCurves where curves.SpreadCurveID == laborType.SpreadCurveID select curves).FirstOrDefault();
			FullWbs thisWBS = inWorkspace.WbsElementsNoMultiWbs.FirstOrDefault(w => w.Id == laborType.WBSID);
			ClinDTO thisClin = inWorkspace.ClinsNoMultiClin.FirstOrDefault(c => c.Id == laborType.CLINID);
			List<string> row = new List<string>();

			switch (inTaskElement.TaskElementType)
			{
				case TaskElementType.Labor:
					row.AddRange(
						new string[]
							{
								laborType.Id.ToString(),
								thisResource != null ? thisResource.ResourceName : string.Empty
							});

					if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
					{
						row.Add(thisBusinessResourceCode != null ? thisBusinessResourceCode.ResourceName : string.Empty);
					}

					row.Add(thisPerfOrg != null ? thisPerfOrg.PerformingOrgName : string.Empty);

					ICollection<string> customFieldValues = CreateCustomFieldRowValues(laborType, customFieldDictionary, laborTypeCustomFieldValueIdMappings, workspaceCustomFields);
					row.AddRange(customFieldValues);
					if (isMulti)
					{
						row.Add(thisWBS != null ? thisWBS.WbsString : string.Empty);
						row.Add(thisClin != null ? thisClin.ClinString : string.Empty);
					}

					row.AddRange(
						new string[]
							{
								laborType.StartDate.Value.ToString("MM/yyyy"),
								laborType.EndDate.Value.ToString("MM/yyyy")
							});

					row.Add(thisSpread.SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(inWorkspace)));

					if (laborType.SpreadType == SpreadType.Cost)
					{
						row.Add("0");  // Percent Spread
						row.Add("0");  // Hours Spread
									   // Cost
						row.Add(laborType.ValueSpread.HasValue ? Utilities.FormatStringWithPrecisionNoComma(laborType.ValueSpread.Value, inWorkspace.CostDecimalPrecision) : "0");
					}
					else
					{

						if (laborType.PercentSpreadLocked)
						{
							row.Add(laborType.PercentSpread.HasValue ? laborType.PercentSpread.Value.ToString("F6") : "0");  // Percent Spread is always capped at 6 DPs
							row.Add(string.Empty); // Hours Spread not being exported
						}
						else
						{
							row.Add(string.Empty);   // Percent Spread not being exported
							row.Add(laborType.ValueSpread.HasValue ? Utilities.FormatStringWithPrecisionNoComma(laborType.ValueSpread.Value, inWorkspace.DecimalPrecision) : "0"); // Hours Spread
						}
						row.Add(string.Empty); // Cost row
					}

					if (isOffload)
					{
						row.Add(laborType.CanOffload.ToString().ToUpper());
					}

					string[] laborSpreadDataDisplayValues = CreateLaborSpreadRowValues(inWorkspace, spreadDateColumnIndices, laborType, inTaskElement.Id, thisResource, thisPerfOrg);
					row.AddRange(laborSpreadDataDisplayValues);

					break;
			}

			return row;
		}

		/// <summary>
		/// Creates the custom field dictionary which uses the values as keys to the custom field dto.
		/// </summary>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="workspaceCustomFieldValues">The workspace custom field values.</param>
		/// <returns>A dictionary with custom field values as keys to their respective custom fields.</returns>
		private static IDictionary<CustomFieldValueDTO, CustomFieldDTO> CreateCustomFieldDictionary(ICollection<CustomFieldDTO> workspaceCustomFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues)
		{
			IDictionary<CustomFieldValueDTO, CustomFieldDTO> taskResourceElementCustomFields = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();

			foreach (CustomFieldValueDTO customFieldValueDto in workspaceCustomFieldValues)
			{
				CustomFieldDTO customFieldDto = workspaceCustomFields.FirstOrDefault(f => f.Id == customFieldValueDto.CustomFieldID);
				if (customFieldDto != null)
				{
					// add to list
					taskResourceElementCustomFields.Add(customFieldValueDto, customFieldDto);
				}
			}

			return taskResourceElementCustomFields;
		}

		/// <summary>
		/// Creates the custom field values as strings for the row.
		/// </summary>
		/// <param name="laborType">The labor type resource for the row.</param>
		/// <param name="allTaskResourcesCustomFields">All task resources custom fields.</param>
		/// <param name="laborTypeCustomFieldValueIdMappings">The labor type custom field value identifier mappings.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <returns>A list of strings that are either string.Empty or the custom field choices made for this labor type resource.</returns>
		private static ICollection<string> CreateCustomFieldRowValues(ResourceTypeDto laborType, IDictionary<CustomFieldValueDTO, CustomFieldDTO> allTaskResourcesCustomFields, Dictionary<int, ICollection<KeyValuePair<int, int>>> laborTypeCustomFieldValueIdMappings, ICollection<CustomFieldDTO> workspaceCustomFields)
		{
			List<string> customFieldValues = new List<string>();

			// Display custom fields (if any)
			int laborTypeId = laborType.Id;
			if (laborTypeCustomFieldValueIdMappings.ContainsKey(laborTypeId))
			{
				ICollection<int> laborTypeCustomFieldValueIds = laborTypeCustomFieldValueIdMappings[laborTypeId].Select(c => c.Value).ToList();
				var laborTypeCustomFields = (from cf in allTaskResourcesCustomFields
											 join id in laborTypeCustomFieldValueIds on cf.Key.CustomFieldValueID equals id
											 select new
											 {
												 CustomFieldValueID = cf.Key.CustomFieldValueID,
												 CustomFieldId = cf.Value.Id,
												 CustomFieldName = cf.Value.CustomFieldName,
												 CustomFieldValueName = cf.Key.CustomFieldValueName,
												 CustomFieldValueDescription = cf.Key.CustomFieldValueDescription
											 }).ToCollection();

				foreach (CustomFieldDTO customField in workspaceCustomFields)
				{
					var laborTypeCustomField = laborTypeCustomFields.SingleOrDefault(cf => cf.CustomFieldId == customField.Id);
					if (laborTypeCustomField == null)
					{
						// no value found for this custom field
						customFieldValues.Add(string.Empty);
					}
					else if (customField.IsOpenEnded)
					{
						customFieldValues.Add(string.Format("{0}", laborTypeCustomField.CustomFieldValueDescription));
					}
					else
					{
						customFieldValues.Add(string.Format("{0} - {1}", laborTypeCustomField.CustomFieldValueName, laborTypeCustomField.CustomFieldValueDescription));
					}
				}
			}
			else
			{
				// no custom fields saved for this labor type
				for (int i = 0; i < workspaceCustomFields.Count; i++)
				{
					customFieldValues.Add(string.Empty);
				}
			}

			return customFieldValues;
		}

		/// <summary>
		/// Creates the labor spread row values.
		/// </summary>
		/// <param name="inWorkspace">The workspace.</param>
		/// <param name="spreadDateColumnIndices">The spread date column indices.</param>
		/// <param name="laborResource">The labor resource.</param>
		/// <param name="taskElementId">The task element identifier.</param>
		/// <param name="resource">The resource.</param>
		/// <param name="performingOrg">The performing org.</param>
		/// <returns>An array of strings representing the labor spread data.</returns>
		private static string[] CreateLaborSpreadRowValues(FullWorkspace inWorkspace, Dictionary<DateTime, int> spreadDateColumnIndices, ResourceTypeDto laborResource, int taskElementId, ResourceDTO resource, PerformingOrgDTO performingOrg)
		{
			int totalSpreadMonths = spreadDateColumnIndices.Count;
			string[] laborSpreadDataDisplayValues = new string[totalSpreadMonths];
			for (int i = 0; i < totalSpreadMonths; i++)
			{
				laborSpreadDataDisplayValues[i] = string.Empty;
			}

			// assemble display values array
			if (laborResource.LaborSpreads != null)
			{
				foreach (ResourceSpreadDto laborSpread in laborResource.LaborSpreads)
				{
					DateTime spreadMonth = laborSpread.LaborSpreadDate;

					if (spreadDateColumnIndices.ContainsKey(spreadMonth))
					{
						int spreadMonthColumnIdx = spreadDateColumnIndices[spreadMonth];

						decimal monthSpreadValue = laborSpread.LaborSpreadValue;

						if (laborResource.SpreadType == SpreadType.Cost)
						{
							laborSpreadDataDisplayValues[spreadMonthColumnIdx] = monthSpreadValue.ToString("F2");
						}
						else
						{
							laborSpreadDataDisplayValues[spreadMonthColumnIdx] = Utilities.FormatStringWithPrecisionNoComma(monthSpreadValue, inWorkspace.DecimalPrecision);
						}
					}
					else
					{

						string resourceName = string.Empty;
						if (resource != null)
						{
							resourceName = resource.ResourceName;
						}

						string performingOrgName = string.Empty;
						if (performingOrg != null)
						{
							performingOrgName = performingOrg.PerformingOrgName;
						}

						log.Warn(
							$"Possible out-of-range labor spread entry detected: taskElementID={taskElementId}, resource={resourceName}, perforg={performingOrgName}, month={spreadMonth.ToMonthString()}");
					}
				}
			}

			return laborSpreadDataDisplayValues;
		}

		/// <summary>
		/// Populates the options list worksheet.
		/// </summary>
		/// <param name="spreadsheet">The spreadsheet.</param>
		/// <param name="allPerformingOrgs">All performing orgs.</param>
		/// <param name="allCurves">All curves.</param>
		/// <param name="allResourceTypes">All resource types.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="workspaceCustomFieldValues">The workspace custom field values.</param>
		/// <param name="workspaceWBSs">The workspace WBSs.</param>
		/// <param name="workspaceClins">The workspace CLINs.</param>
		/// <param name="inWorkspace">The workspace.</param>
		private static void PopulateOptionsList(SpreadsheetDocument spreadsheet, IReadOnlyCollection<PerformingOrgDTO> allPerformingOrgs, ICollection<SpreadCurveModelView> allCurves, ICollection<ResourceDTO> allResourceTypes,
			ICollection<CustomFieldDTO> workspaceCustomFields, ICollection<CustomFieldValueDTO> workspaceCustomFieldValues, IReadOnlyCollection<FullWbs> workspaceWBSs, IReadOnlyCollection<FullClin> workspaceClins,
			FullWorkspace inWorkspace, ICollection<ResourceDTO> allBusinessResourceCodes)
		{
			// Create collections of strings for each row in the export file
			ExcelExportWorksheet optionsListWorksheet = new ExcelExportWorksheet(ImportExportConstants.OPTIONS_LISTS);

			// Options List Column Headers
			List<string> headerValues = new List<string>
			{
				ImportExportConstants.RESOURCE_COLUMN_HEADER,
				ImportExportConstants.PERF_ORG_COLUMN_HEADER,
				ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER,
				ImportExportConstants.WBS_COLUMN_HEADER,
				ImportExportConstants.CLIN_COLUMN_HEADER,
				ImportExportConstants.OFFLOAD_COLUMN_HEADER
			};

			if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
			{
				headerValues.Add(ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER);
			}

			string[] offloadOptions = new string[] { "TRUE", "FALSE" };

			int maxRows = Math.Max(allCurves.Count, Math.Max(allPerformingOrgs.Count, Math.Max(workspaceWBSs.Count, Math.Max(workspaceClins.Count, allResourceTypes.Count))));

			if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
			{
				maxRows = Math.Max(maxRows, allBusinessResourceCodes.Count);
			}

			#region Custom Fields

			List<List<string>> customFieldValueTable = new List<List<string>>();

			// keep a list of defined names for each custom field 
			Dictionary<string, string> customFieldDefinedNames = new Dictionary<string, string>();

			foreach (CustomFieldDTO customField in workspaceCustomFields.Where(x => !x.IsOpenEnded))
			{
				headerValues.Add(customField.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName));    // Options List column header

				// Get all values for the current field
				ICollection<CustomFieldValueDTO> customFieldValuesToCopy = workspaceCustomFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection();

				List<string> valueList = new List<string>();

				// If there are values to copy, let's do it
				if (customFieldValuesToCopy.Any())
				{
					// Iterate over each value
					foreach (CustomFieldValueDTO customFieldValue in customFieldValuesToCopy)
					{
						valueList.Add(String.Format("{0} - {1}", customFieldValue.CustomFieldValueName, customFieldValue.CustomFieldValueDescription));
					}
				}
				customFieldValueTable.Add(valueList);
				maxRows = maxRows > valueList.Count ? maxRows : valueList.Count;
				string colName = ExcelUtilities.GetColumnNameFromColumnIndex(headerValues.Count - 1);
				string definedNameText = (valueList.Count > 1) ? "'" + ImportExportConstants.OPTIONS_LISTS + "'!$" + colName + "$2:$" + colName + "$" + (valueList.Count + 1)
															   : "'" + ImportExportConstants.OPTIONS_LISTS + "'!$" + colName + "$2";
				// Create a DefinedName to be used for data validation
				// Defined Names must start with an underscore, or letter and cannot contain 
				// spaces or other special characters.  So, create the DefinedName as "CustomField_" followed by the FieldID.
				customFieldDefinedNames.Add(ImportExportConstants.CUSTOM_FIELD_DEFINED_NAME_PREFIX + customField.Id, definedNameText);
			}

			#endregion Custom Fields

			optionsListWorksheet.Add(headerValues);   // Add option list header row

			// Add option value rows
			for (int i = 0; i < maxRows; i++)
			{
				List<string> optionValues = new List<string>
				{
					allResourceTypes.Count > i ? allResourceTypes.ElementAt(i).ResourceName : string.Empty,
					allPerformingOrgs.Count > i ? allPerformingOrgs.ElementAt(i).PerformingOrgName : string.Empty,
					allCurves.Count > i ? allCurves.ElementAt(i).SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(inWorkspace)) : string.Empty,
					workspaceWBSs.Count > i ? workspaceWBSs.ElementAt(i).WbsString : string.Empty,
					workspaceClins.Count > i ? workspaceClins.ElementAt(i).ClinString : string.Empty,
					offloadOptions.Length > i ? offloadOptions.ElementAt(i) : string.Empty
				};

				if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
				{
					optionValues.Add(allBusinessResourceCodes.Count > i ? allBusinessResourceCodes.ElementAt(i).ResourceName : string.Empty);
				}

				// include custom field values
				foreach (List<string> valueList in customFieldValueTable)
				{
					optionValues.Add(valueList.Count > i ? valueList[i] : string.Empty);
				}

				optionsListWorksheet.Add(optionValues);
			}

			// Export the Options List headers and data to row 1 of the "Options List" worksheet
			WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, optionsListWorksheet.WorksheetName);
			ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, optionsListWorksheet, 1);

			// Adjust existing Defined Names
			Dictionary<string, int> lengths = new Dictionary<string, int>()
			{
				{ ImportExportConstants.RESOURCES, allResourceTypes.Count },
				{ ImportExportConstants.PERFORGS, allPerformingOrgs.Count },
				{ ImportExportConstants.SPREAD_CURVES, allCurves.Count },
				{ ImportExportConstants.WBSS, workspaceWBSs.Count },
				{ ImportExportConstants.CLINS, workspaceClins.Count },
				{ ImportExportConstants.OFFLOAD, offloadOptions.Length }
			};

			if (Utilities.IsBRCEnabledForWorkspace(inWorkspace.Shortname))
			{
				lengths.Add(ImportExportConstants.BUSINESS_RESOURCE_CODES, allBusinessResourceCodes.Count);
			}

			ExcelExporter.AdjustDefinedNames(spreadsheet, lengths);
			// Add Defined Names for each custom field
			ExcelExporter.AddDefinedNames(spreadsheet, customFieldDefinedNames);
		}

		#endregion Private Functions
	}
}
