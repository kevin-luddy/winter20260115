// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using DocumentFormat.OpenXml.Packaging;
	using GenBOE.ActionLogic.Common;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Used for importing new genBOE Resource Spread elements from an Excel file
	/// </summary>
	public class LaborTypeAndSpreadImporter
	{
		private IResourceDTODataLoader resourceDTODataLoader;
		private IPerformingOrgDTODataLoader perfOrgLoader;
		private ICommonDataMapper commonDataMapper;

		#region Constants
		// Individual column names
		internal const string LABOR_TYPE_ID_COL = "Labor Type ID";
		internal const string PERFORMING_ORG_COL = "Performing Org";
		internal const string PERCENT_SPREAD_COL = "% Spread";
		internal const string HOURS_SPREAD_COL = "Hours Spread";
		internal const string EP_SPREAD_COL = "EP Spread";
		internal const string COST_COL = "Cost";
		internal const string RESOURCE_CLIN_HEADER = "CLIN";
		internal const string RESOURCE_WBS_HEADER = "WBS";
		internal const string IMPORT_TAB = "Labor Types";

		// Array of the columns that must be contained in the imported file
		private readonly string[] REQUIRED_COLUMNS = new string[] { LABOR_TYPE_ID_COL, ImportExportConstants.RESOURCE_COLUMN_HEADER, PERFORMING_ORG_COL, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER, PERCENT_SPREAD_COL, COST_COL };
		private readonly string[] REQUIRED_COLUMNS_BRC_ENABLED = new string[] { LABOR_TYPE_ID_COL, ImportExportConstants.RESOURCE_COLUMN_HEADER, ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER, PERFORMING_ORG_COL, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER, PERCENT_SPREAD_COL, COST_COL };
		#endregion Constants

		#region Public Functions

		/// <summary>
		/// Initializes a new instance of the <see cref="LaborTypeAndSpreadImporter"/> class.
		/// </summary>
		/// <param name="inIResourceDTODataLoader">The resource dto data loader.</param>
		/// <param name="perfOrgLoader">The perf org loader.</param>
		/// <param name="inICommonDataMapper">The common data mapper.</param>
		public LaborTypeAndSpreadImporter(
			IResourceDTODataLoader inIResourceDTODataLoader,
			IPerformingOrgDTODataLoader perfOrgLoader,
			ICommonDataMapper inICommonDataMapper)
		{
			this.resourceDTODataLoader = inIResourceDTODataLoader;
			this.perfOrgLoader = perfOrgLoader;
			this.commonDataMapper = inICommonDataMapper;
		}

		public Collection<ImportedLaborType> ImportLaborTypeFromExcelFile(Stream inExcelFileStream, BoeTaskElementDTO inTaskElement, FullWorkspace inWorkspace, Boolean newOnly)
		{

			if (inExcelFileStream == null)
			{
				throw new ArgumentNullException(nameof(inExcelFileStream));
			}

			if (inTaskElement == null)
			{
				throw new ArgumentNullException(nameof(inTaskElement));
			}

			if (inWorkspace == null)
			{
				throw new ArgumentNullException(nameof(inWorkspace));
			}

			bool isMulti = inWorkspace.Boes.First(b => b.Id == inTaskElement.BoeID).IsMultiClinWbs;
			bool isOffload = inWorkspace.ProjectMapType == ProjectMapType.StandardWithOffload;
			try
			{
				Collection<ImportedLaborType> importResults;

				// Open the document as read-only.
				using (SpreadsheetDocument document = SpreadsheetDocument.Open(inExcelFileStream, false))
				{
					// add column header "identifiers" for each spread-month-data column
					List<string> columnsToRetrieve;
					List<string> requiredColumns;

					if (Utilities.IsBRCEnabledForSystem)
					{
						columnsToRetrieve = this.REQUIRED_COLUMNS_BRC_ENABLED.Union(FindSpreadDatesHeaders(inTaskElement)).ToList();
						requiredColumns = new List<string>(this.REQUIRED_COLUMNS_BRC_ENABLED);
					}
					else
					{
						columnsToRetrieve = this.REQUIRED_COLUMNS.Union(FindSpreadDatesHeaders(inTaskElement)).ToList();
						requiredColumns = new List<string>(this.REQUIRED_COLUMNS);
					}

					if (isMulti)
					{
						columnsToRetrieve.Add(RESOURCE_WBS_HEADER);
						columnsToRetrieve.Add(RESOURCE_CLIN_HEADER);
					}

					if (isOffload)
					{
						columnsToRetrieve.Add(ImportExportConstants.OFFLOAD_COLUMN_HEADER);
						requiredColumns.Add(ImportExportConstants.OFFLOAD_COLUMN_HEADER);
					}

					if (FullObjectHelper.ShowEquivalentPersonsOption && inWorkspace.IsUsingEquivalentPerson)
					{
						requiredColumns.Add(EP_SPREAD_COL);
						columnsToRetrieve.Add(EP_SPREAD_COL);
					}
					else
					{
						requiredColumns.Add(HOURS_SPREAD_COL);
						columnsToRetrieve.Add(HOURS_SPREAD_COL);
					}

					// Add the custom fields to required and columns to retrieve
					ICollection<CustomFieldDTO> workspaceCustomFields = inWorkspace.CustomFields.Where(cf => cf.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay).ToCollection();
					foreach (CustomFieldDTO customField in workspaceCustomFields)
					{
						if (customField.CustomFieldRequired)
						{
							requiredColumns.Add(ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName));
						}

						columnsToRetrieve.Add(customField.CustomFieldRequired ? ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName) : ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName));
					}

					// BOEJ-3307 - When spread months are invalid, flag them, display failures and do not import.
					string[] allColumns = columnsToRetrieve.ToArray();
					ICollection<string> columnHeaders = ExcelUtilities.GetAllColumnHeaderStrings(document, IMPORT_TAB);
					if (columnHeaders.Any(x => IsInvalidSpreadDateColumnHeader(x, allColumns)))
					{
						// The import contains spread month(s) that are outside the task start/end date range
						importResults = new Collection<ImportedLaborType> {
							new ImportedLaborType {
								ImportTypes = new Collection<LaborTypeImportResult> { LaborTypeImportResult.SpreadMonthColumnInvalid },
								SpreadCurveID = SpreadCurves.None,
								SpreadType = SpreadType.NotSet
							}
						};
					}
					else
					{
						ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, IMPORT_TAB, requiredColumns.ToArray(), allColumns);

						// Turn each row into a DTO object and return the collection
						importResults = this.CreateImportedLaborTypes(allRows, inTaskElement, inWorkspace, newOnly, workspaceCustomFields, isMulti, isOffload);
					}
				}

				return importResults;
			}
			catch (FileFormatException)
			{
				throw new NotExcelFileException("Imported file was an incorrect format.");
			}
		}

		/// <summary>
		/// Returns true, if column header is MM/yyyy date, but not present in allColumns array (i.e. outside task start/end date range).
		/// </summary>
		/// <returns>true if column header is NOT a valid spread date header; false otherwise, i.e. column header is valid.</returns>
		public static bool IsInvalidSpreadDateColumnHeader(string columnHeader, string[] allColumns)
		{
			if (!allColumns.Contains(columnHeader))
			{
				DateTime spreadDate;
				if (DateTime.TryParseExact(columnHeader, "MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out spreadDate))
				{
					return true;   // spread date header is outside start/end date boundaries
				}
			}

			return false;    // success
		}

		#endregion Public Functions

		#region Private Functions

		/// <summary>
		/// Finds the spread dates headers.
		/// </summary>
		/// <param name="inTaskElement">The task element.</param>
		/// <returns>An enumerable list of strings representing the spread dates that should be in the headers.</returns>
		private static ICollection<string> FindSpreadDatesHeaders(BoeTaskElementDTO inTaskElement)
		{
			ICollection<string> completeSpreadDatesHeaders;
			if (inTaskElement.taskElementLabors != null && inTaskElement.taskElementLabors.Any())
			{
				// generate the headers from the task element's labor resources
				completeSpreadDatesHeaders = inTaskElement.taskElementLabors.GetSpreadDatesFull().Select(d => d.ToString("MM/yyyy")).ToList();
			}
			else
			{
				// generate the headers from the task element's start/end date
				DateTime spreadStartDate = inTaskElement.StartDate.Value;
				DateTime spreadEndDate = inTaskElement.EndDate.Value;

				ICollection<string> spreadDates = new List<string>();

				// display the header (all dates across the spread)
				for (DateTime dt = spreadStartDate.Date; dt.Date <= spreadEndDate; dt = dt.AddMonths(1))
				{
					spreadDates.Add(dt.ToString("MM/yyyy"));
				}

				completeSpreadDatesHeaders = spreadDates;
			}

			return completeSpreadDatesHeaders;
		}

		/// <summary>
		/// Creates the imported labor types.
		/// </summary>
		/// <param name="allRows">All of the rows.</param>
		/// <param name="taskElement">The task element.</param>
		/// <param name="workspace">The workspace.</param>
		/// <param name="newOnly">if set to <c>true</c> only import new rows.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="isMulti">Whether the BOE is a Multi-Clin/WBS BOE or not.</param>
		/// <param name="isOffload">True if we should show the Offload column; otherwise false.</param>
		/// <returns>List of imported Labor Types.</returns>
		private Collection<ImportedLaborType> CreateImportedLaborTypes(
			ICollection<Dictionary<string, string>> allRows,
			BoeTaskElementDTO taskElement,
			FullWorkspace workspace,
			Boolean newOnly,
			ICollection<CustomFieldDTO> workspaceCustomFields, Boolean isMulti,
			bool isOffload)
		{
			if (allRows == null)
			{
				throw new ArgumentNullException(nameof(allRows));
			}

			// Create the collection to return
			Collection<ImportedLaborType> toReturn = new Collection<ImportedLaborType>();
			//Workspace Clins and WBS
			Collection<FullWbs> wsWBS = new Collection<FullWbs>(workspace.WbsElementsNoMultiWbs.ToCollection());
			Collection<ClinDTO> wsClins = new Collection<ClinDTO>(workspace.ClinsNoMultiClin.ToCollection<ClinDTO>());
			if (allRows.Any())
			{
				// Get all Labor Types currently in the workspace and convert 
				Collection<ResourceTypeDto> existingLaborResources = taskElement.taskElementLabors;

				// int to keep track of negative ids for new Custom Fields so there are no duplicates
				int newCustomFieldIndex = -1;

				foreach (Dictionary<string, string> row in allRows)
				{
					ImportedLaborType toAdd;
					int? laborTypeID = null;

					if (row.ContainsKey(LABOR_TYPE_ID_COL))
					{
						laborTypeID = int.Parse(row[LABOR_TYPE_ID_COL]);
					}

					ResourceTypeDto existingLT = null;

					//check if it's new
					if (laborTypeID.HasValue)
					{
						existingLT = (from LT in existingLaborResources
									  where LT.Id == laborTypeID
									  select LT)
										  .FirstOrDefault();

						if (existingLT == null)
						{
							if (!newOnly)
							{
								// out of sync, the import contains an existing labor resource that is not in the current task element
								toAdd = new ImportedLaborType();
								toAdd.ImportTypes.Add(LaborTypeImportResult.ResourceTypeIDMissingOrInvalid);
								toAdd.LaborTypeID = laborTypeID;
								toAdd.SpreadCurveID = SpreadCurves.None;
								toAdd.SpreadType = SpreadType.NotSet;
								toReturn.Add(toAdd);
							}

							// early ending of this iteration of the foreach loop
							continue;
						}
					}

					if (existingLT != null)
					{
						toAdd = new ImportedLaborType(existingLT, this.resourceDTODataLoader, this.perfOrgLoader, LaborTypeImportResult.UpdateLaborType);
						SpreadCurveModelView curveModelView = (from curve in this.commonDataMapper.getSpreadCurve() where curve.SpreadCurveID == toAdd.SpreadCurveID select curve).FirstOrDefault();
						if (curveModelView == null)
						{
							toAdd.Curve = string.Empty;
							toAdd.ImportTypes.Add(LaborTypeImportResult.MissingData);
						}
						else
						{
							toAdd.Curve = curveModelView.SpreadCurveName;
						}

						toAdd = this.update(toAdd,
							this.ConstructImportfromFile(row, taskElement, workspace, existingLT, workspaceCustomFields,
								wsWBS, wsClins, isMulti, isOffload, ref newCustomFieldIndex), workspaceCustomFields.Any());
					}
					else
					{
						toAdd = this.ConstructImportfromFile(row, taskElement, workspace, existingLT,
							workspaceCustomFields, wsWBS, wsClins, isMulti, isOffload, ref newCustomFieldIndex);
						toAdd.ImportTypes.Add(LaborTypeImportResult.AddLaborType);
					}

					//clean up imports
					if (!toAdd.ImportTypes.Contains(LaborTypeImportResult.UnChanged))
					{
						// check before "confirm" removes it
						bool isUpdate = toAdd.ImportTypes.Contains(LaborTypeImportResult.UpdateLaborType);

						// Change the Types so that types that can only exist on their own are alone.
						this.ConfirmTypeIntegrity(toAdd);

						// if newOnly then don't add updated ones.
						if (newOnly)
						{
							if (!isUpdate)
							{
								toReturn.Add(toAdd);
							}
						}
						// else add all of them.
						else
						{
							toReturn.Add(toAdd);
						}
					}
					else
					{
						toReturn.Add(toAdd);
					}

					if (toAdd.StartDate == DateTime.MinValue || toAdd.StartDate == DateTime.MaxValue)
					{
						throw new DateImportException("Start Date must be supplied.");
					}
					else if (toAdd.EndDate == DateTime.MinValue || toAdd.EndDate == DateTime.MaxValue)
					{
						throw new DateImportException("End Date must be supplied.");
					}
					else if (toAdd.StartDate != null && toAdd.EndDate != null)
					{
						DateTime startDate = toAdd.StartDate.Value;
						DateTime endDate = toAdd.EndDate.Value;

						if (startDate > endDate)
						{
							throw new DateImportException("End Date cannot be before Start Date.");
						}
					}
				} // end foreach row
			}

			return toReturn;
		}

		/// <summary>
		/// Verifies that the total Sum Of the Resource fits validation criteria.  
		/// </summary>
		/// <param name="listOfSpread">Values being imported</param>
		/// <param name="listOfOrignalValues">Original spreads</param>
		/// <param name="typeOfSpread">What type of spread is this.</param>
		private static void VerifyTotalSumOfResource(Collection<ImportedLaborSpread> listOfSpread, Collection<ResourceSpreadDto> listOfOrignalValues, SpreadType typeOfSpread)
		{

			Decimal total = AddAllSpreadsOrigReplaceWithImports(listOfOrignalValues, listOfSpread);

			if (typeOfSpread == SpreadType.Cost)
			{
				if (!ImportUtils.IsSpread12Digits((long?)total))
				{
					ChangeUpdateSpreadToInvalid(listOfSpread, LaborSpreadImportResult.ResourceCostValueTooLarge);
				}
			}
			else if (typeOfSpread == SpreadType.Hours)
			{
				if (!ImportUtils.IsSpread10Digits((long?)total))
				{
					ChangeUpdateSpreadToInvalid(listOfSpread, LaborSpreadImportResult.ResourceHourValueTooLarge);
				}
			}
		}

		/// <summary>
		/// Adds the new imported numbers and the originals replaces originals where they are being update
		/// to get a sum for the resource.
		/// </summary>
		/// <param name="existingLaborResources">The existing labor resources.</param>
		/// <param name="toReturn">The collection of imported labor spreads to return.</param>
		/// <returns>The sum of all merged resource spreads.</returns>
		private static Decimal AddAllSpreadsOrigReplaceWithImports(Collection<ResourceSpreadDto> existingLaborResources, Collection<ImportedLaborSpread> toReturn)
		{
			List<decimal> listOfResourceSpreadsToSum = new List<decimal>();

			// Add all of the updated values to the list to sum
			foreach (ImportedLaborSpread importing in toReturn)
			{
				if (importing.ImportTypes.Contains(LaborSpreadImportResult.UpdateSpread))
				{
					listOfResourceSpreadsToSum.Add(importing.LaborSpreadValue);
				}
			}

			// Only add existing spread values if not in the updated values to sum
			foreach (ResourceSpreadDto spreadBefore in existingLaborResources)
			{
				bool needToAddThisSpreadInTotal = true;
				foreach (ImportedLaborSpread importing in toReturn)
				{
					if (importing.ImportTypes.Contains(LaborSpreadImportResult.UpdateSpread))
					{
						if (importing.LaborSpreadDate.Equals(spreadBefore.LaborSpreadDate))
						{
							//its in the list that will be updated so that value will be summed not the original value
							needToAddThisSpreadInTotal = false;
						}
					}
				}
				if (needToAddThisSpreadInTotal)
				{
					listOfResourceSpreadsToSum.Add(spreadBefore.LaborSpreadValue);
				}
			}

			Decimal sum = listOfResourceSpreadsToSum.Sum();

			return sum;
		}

		/// <summary>
		/// Updates the Import type to the passed error code.
		/// </summary>
		/// <param name="toReturnHours">To return hours.</param>
		/// <param name="errorCode">The error code.</param>
		private static void ChangeUpdateSpreadToInvalid(Collection<ImportedLaborSpread> toReturnHours, LaborSpreadImportResult errorCode)
		{
			foreach (ImportedLaborSpread wasGoingToUpdate in toReturnHours)
			{
				wasGoingToUpdate.ImportTypes.Clear();
				wasGoingToUpdate.ImportTypes.Add(errorCode);
			}
		}

		/// <summary>
		/// Confirms the type integrity.
		/// </summary>
		/// <param name="toConfirm">The imported Labor Type data to confirm.</param>
		/// <returns><c>True</c> if Labor Type data integrity is all right; otherwise, <c>False</c>.</returns>
		private void ConfirmTypeIntegrity(ImportedLaborType toConfirm)
		{
			bool remove = false;

			if (toConfirm.ImportTypes.Contains(LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange) ||
				toConfirm.ImportTypes.Contains(LaborTypeImportResult.MissingData) ||
				toConfirm.ImportTypes.Contains(LaborTypeImportResult.InvalidData) ||
				toConfirm.ImportTypes.Contains(LaborTypeImportResult.SpreadMonthColumnInvalid) ||
				toConfirm.ImportTypes.Contains(LaborTypeImportResult.SpreadMonthValueOutsideDateRange))
			{
				remove = true;
			}

			if (toConfirm.ImportTypes.Contains(LaborTypeImportResult.HoursSpreadInvalid))
			{
				remove = true;
			}

			if (toConfirm.ImportTypes.Contains(LaborTypeImportResult.RateTypeSpreadTypeAgreement))
			{
				remove = true;
			}
			if (toConfirm.ImportTypes.Contains(LaborTypeImportResult.ResourceMultiValuesInvalid))
			{
				remove = true;
			}

			if (remove)
			{
				toConfirm.ImportTypes.Remove(LaborTypeImportResult.AddLaborType);
				toConfirm.ImportTypes.Remove(LaborTypeImportResult.UpdateLaborType);
			}
		}

		/// <summary>
		/// Applies a update to the ImportedLaborType if there is nothing to update it returns null.
		/// </summary>
		/// <param name="inExistingBOELaborType">Existing Resource from the DB.</param>
		/// <param name="inImportedBOELaborType">Imported resource from excel.</param>
		/// <param name="isAnyCustomFields">True if there are any labor type custom fields in the workspace.</param>
		/// <returns>Updated ImportedLaborType with imported values applied.</returns>
		private ImportedLaborType update(ImportedLaborType inExistingBOELaborType, ImportedLaborType inImportedBOELaborType, bool isAnyCustomFields)
		{
			bool different = false;

			ImportedLaborType updatedLT = inExistingBOELaborType;
			updatedLT.ImportTypes = inImportedBOELaborType.ImportTypes;

			if (isAnyCustomFields)
			{
				foreach (CustomFieldValueContainer container in inImportedBOELaborType.CustomFieldValueContainers)
				{
					// Get the original update date
					CustomFieldValueContainer original = inExistingBOELaborType.CustomFieldValueContainers.FirstOrDefault(c => c.ContainerID == container.ContainerID);
					if (original != null)
					{
						container.UpdateDate = original.UpdateDate;
						if ((!container.IsOpenEnded && container.CustomFieldValueID == original.CustomFieldValueID) || (container.IsOpenEnded && container.OpenEndedValue == original.OpenEndedValue))
						{
							// There was no change, so make sure the update type is set to none
							container.Updateable = UpdateType.None;
						}
					}

					if (container.Updateable != UpdateType.None)
					{
						different = true;
					}
				}

				if (different)
				{
					inExistingBOELaborType.CustomFieldValueContainers = inImportedBOELaborType.CustomFieldValueContainers;
				}
			}

			if (inExistingBOELaborType.ResourceID != inImportedBOELaborType.ResourceID)
			{
				updatedLT.ResourceID = inImportedBOELaborType.ResourceID.Value;
				updatedLT.Resource = inImportedBOELaborType.Resource;
				updatedLT.SpreadType = inImportedBOELaborType.SpreadType;
				different = true;
			}

			if (inExistingBOELaborType.BusinessResourceCodeID != inImportedBOELaborType.BusinessResourceCodeID)
			{
				updatedLT.BusinessResourceCodeID = inImportedBOELaborType.BusinessResourceCodeID.Value;
				updatedLT.BusinessResourceCode = inImportedBOELaborType.BusinessResourceCode;
				updatedLT.SpreadType = inImportedBOELaborType.SpreadType;
				different = true;
			}

			if (inExistingBOELaborType.PerformingOrgID != inImportedBOELaborType.PerformingOrgID)
			{

				updatedLT.PerformingOrgID = inImportedBOELaborType.PerformingOrgID.Value;
				updatedLT.PerformingOrg = inImportedBOELaborType.PerformingOrg;
				different = true;
			}

			if (inExistingBOELaborType.CLINID != inImportedBOELaborType.CLINID)
			{
				updatedLT.CLINID = inImportedBOELaborType.CLINID;
				different = true;
			}

			if (inExistingBOELaborType.WBSID != inImportedBOELaborType.WBSID)
			{
				updatedLT.WBSID = inImportedBOELaborType.WBSID;
				different = true;
			}

			if (inExistingBOELaborType.CanOffload != inImportedBOELaborType.CanOffload)
			{
				updatedLT.CanOffload = inImportedBOELaborType.CanOffload;
				different = true;
			}

			if (!inImportedBOELaborType.ImportTypes.Contains(LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange))
			{
				if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(inExistingBOELaborType.StartDate), DateTimePrecision.Month) != GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(inImportedBOELaborType.StartDate), DateTimePrecision.Month))
				{
					updatedLT.StartDateValue = inImportedBOELaborType.StartDate.Value;
					different = true;
				}

				if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(inExistingBOELaborType.EndDate), DateTimePrecision.Month) != GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(inImportedBOELaborType.EndDate), DateTimePrecision.Month))
				{
					updatedLT.EndDateValue = inImportedBOELaborType.EndDate.Value;
					different = true;
				}
			}

			if (inExistingBOELaborType.SpreadCurveID != inImportedBOELaborType.SpreadCurveID)
			{
				updatedLT.SpreadCurveID = inImportedBOELaborType.SpreadCurveID;
				updatedLT.Curve = inImportedBOELaborType.Curve;

				different = true;

				// if spread curve selection changes to Discrete Cost, then we need to "erase" all spread-related values
				if (updatedLT.SpreadCurveID.HasValue && updatedLT.SpreadCurveID.Value == SpreadCurves.DiscreteCost)
				{
					updatedLT.ValueSpread = 0L;
					updatedLT.HourSpreadLocked = true;
					updatedLT.PercentSpread = 0m;
					updatedLT.PercentSpreadLocked = true;

					// also update the import data reference (which is displayed on the UI for "to-be-updated" entries)
					if (!inImportedBOELaborType.ImportTypes.Contains(LaborTypeImportResult.RateTypeSpreadTypeAgreement))  // is this being updated?
					{
						inImportedBOELaborType.ValueSpread = 0L;
						inImportedBOELaborType.HourSpreadLocked = true;
						inImportedBOELaborType.PercentSpread = 0m;
						inImportedBOELaborType.PercentSpreadLocked = true;
					}

					if (updatedLT.LaborSpreads != null)
					{
						foreach (ResourceSpreadDto spread in updatedLT.LaborSpreads)
						{
							spread.Updateable = UpdateType.Deleted;
						}
					}
				}
			}

			//Don't bother looking at percent/spread values if the imported row is discrete
			if (inImportedBOELaborType.SpreadCurveID != SpreadCurves.DiscreteCost && inImportedBOELaborType.SpreadCurveID != SpreadCurves.DiscreteHours)
			{
				if (inExistingBOELaborType.PercentSpread != inImportedBOELaborType.PercentSpread
					&& inImportedBOELaborType.PercentSpreadLocked
					&& inImportedBOELaborType.PercentSpread != null)
				{
					updatedLT.PercentSpread = inImportedBOELaborType.PercentSpread;
					different = true;
				}

				decimal? importValueSpread = inImportedBOELaborType.ValueSpread;

				if (inExistingBOELaborType.ValueSpread != importValueSpread
					&& (inImportedBOELaborType.HourSpreadLocked || inImportedBOELaborType.SpreadType == SpreadType.Cost)
					&& inImportedBOELaborType.ValueSpread != null && !inImportedBOELaborType.ImportTypes.Contains(LaborTypeImportResult.HoursSpreadInvalid)
					&& !inImportedBOELaborType.ImportTypes.Contains(LaborTypeImportResult.CostDecimalPrecisionViolation))
				{
					updatedLT.ValueSpread = inImportedBOELaborType.ValueSpread;
					different = true;
				}
			}

			// for existing LT, the locked values shouldn't be enough to mark it as different but we need to take in the newly adjusted values anyway
			updatedLT.PercentSpreadLocked = inImportedBOELaborType.PercentSpreadLocked;
			updatedLT.HourSpreadLocked = inImportedBOELaborType.HourSpreadLocked;

			// If no import problems on labor type, and either the labor type values are different or the labor spreads are different
			if (!updatedLT.ImportTypes.Any() && (different || inImportedBOELaborType.ImportedLaborSpreads.Any()))
			{
				updatedLT.ImportTypes.Add(LaborTypeImportResult.UpdateLaborType);
			}
			else
			{
				updatedLT.ImportTypes.Add(LaborTypeImportResult.UnChanged);
			}

			updatedLT.ImportedLaborSpreads = inImportedBOELaborType.ImportedLaborSpreads;

			return updatedLT;
		}

		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private ImportedLaborType ConstructImportfromFile(Dictionary<string, string> importfromfile, BoeTaskElementDTO inTaskElement,
			FullWorkspace inWorkspace, ResourceTypeDto existingResource, ICollection<CustomFieldDTO> workspaceCustomFields, Collection<FullWbs> wsWbs, Collection<ClinDTO> wsClins,
			bool isMulti, bool isOffload, ref int newCustomFieldIndex)
		{
			ImportedLaborType toReturn = new ImportedLaborType();

			DateTime startDate;
			DateTime endDate;
			DateTime? importStartDate = null;
			DateTime? importEndDate = null;
			decimal percentSpread = 0;
			SpreadCurves? spreadCurveSelection = null;

			#region Labor Type
			if (importfromfile.ContainsKey(LABOR_TYPE_ID_COL) && !String.IsNullOrEmpty(importfromfile[LABOR_TYPE_ID_COL]))
			{
				toReturn.Id = int.Parse(importfromfile[LABOR_TYPE_ID_COL]);
			}

			if (!importfromfile.ContainsKey(ImportExportConstants.RESOURCE_COLUMN_HEADER) || String.IsNullOrEmpty(importfromfile[ImportExportConstants.RESOURCE_COLUMN_HEADER]))
			{
				toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
			}
			else
			{
				ICollection<ResourceDTO> resourcelist = ImportExportUtilities.GetResourcesBasedOnCompanyMode(this.resourceDTODataLoader.GetByListId(inWorkspace.ResourceListID), false);
				ResourceDTO resource = (from resourceToGet in resourcelist where importfromfile[ImportExportConstants.RESOURCE_COLUMN_HEADER] == (resourceToGet.ResourceDesc) select resourceToGet).FirstOrDefault();

				if (resource != null)
				{
					toReturn.ResourceID = resource.Id;
					toReturn.Resource = importfromfile[ImportExportConstants.RESOURCE_COLUMN_HEADER];

					// validate agreement between spread curve selection and resource rate type (either both cost or both hours)
					if (importfromfile.ContainsKey(ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER))
					{
						string spreadCurveText = importfromfile[ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER];
						if (!string.IsNullOrEmpty(spreadCurveText))
						{
							spreadCurveText = spreadCurveText.Replace(FullObjectHelper.HoursLabel(inWorkspace), "Hours");
						}
						spreadCurveSelection = spreadCurveText.GetEnumeratedValue<SpreadCurves>(SpreadCurves.None);
						if ((spreadCurveSelection == SpreadCurves.DiscreteCost && resource.RateType == RateType.Hours) ||
							(spreadCurveSelection == SpreadCurves.DiscreteHours && resource.RateType == RateType.Cost))
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.RateTypeSpreadTypeAgreement);
						}
						else
						{
							toReturn.SpreadType = resource.RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost;
						}
					}
				}
				else
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
				}

				if (Utilities.IsBRCEnabledForSystem)
				{
					if (!importfromfile.ContainsKey(ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER) || String.IsNullOrEmpty(importfromfile[ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER]))
					{
						toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
					}
					else
					{
						ICollection<ResourceDTO> businessResourceCodeList = ImportExportUtilities.GetResourcesBasedOnCompanyMode(this.resourceDTODataLoader.GetByListId(inWorkspace.ResourceListID), true);
						ResourceDTO businessResourceCode = (from brcToGet in businessResourceCodeList where importfromfile[ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER] == (brcToGet.ResourceDesc) select brcToGet).FirstOrDefault();

						if (businessResourceCode != null)
						{
							toReturn.BusinessResourceCodeID = businessResourceCode.Id;
							toReturn.BusinessResourceCode = importfromfile[ImportExportConstants.BUSINESS_RESOURCE_CODE_COLUMN_HEADER];

							// validate agreement between spread curve selection and resource rate type (either both cost or both hours)
							if (importfromfile.ContainsKey(ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER))
							{
								string spreadCurveText = importfromfile[ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER];
								if (!string.IsNullOrEmpty(spreadCurveText))
								{
									spreadCurveText = spreadCurveText.Replace(FullObjectHelper.HoursLabel(inWorkspace), "Hours");
								}
								spreadCurveSelection = spreadCurveText.GetEnumeratedValue<SpreadCurves>(SpreadCurves.None);
								if ((spreadCurveSelection == SpreadCurves.DiscreteCost && resource.RateType == RateType.Hours) ||
									(spreadCurveSelection == SpreadCurves.DiscreteHours && resource.RateType == RateType.Cost))
								{
									toReturn.ImportTypes.Add(LaborTypeImportResult.RateTypeSpreadTypeAgreement);
								}
								else
								{
									toReturn.SpreadType = resource.RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost;
								}
							}
						}
						else
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
						}
					}
				}
			}

			if (!importfromfile.ContainsKey(PERFORMING_ORG_COL) || String.IsNullOrEmpty(importfromfile[PERFORMING_ORG_COL]))
			{
				toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
			}
			else
			{
				IReadOnlyCollection<PerformingOrgDTO> POlist = inWorkspace.PerformingOrgsForWsList;
				PerformingOrgDTO PO = (from poToGet in POlist where importfromfile[PERFORMING_ORG_COL] == (poToGet.PerformingOrgName + " - " + poToGet.PerformingOrgDesc) select poToGet).FirstOrDefault();

				if (PO != null)
				{
					toReturn.PerformingOrgID = PO.Id;
					toReturn.PerformingOrg = importfromfile[PERFORMING_ORG_COL];
				}
				else
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
				}
			}

			if (!importfromfile.ContainsKey(ImportExportConstants.START_DATE_COLUMN_HEADER) || !DateTime.TryParse(importfromfile[ImportExportConstants.START_DATE_COLUMN_HEADER], out startDate))
			{
				toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
			}
			else
			{
				importStartDate = startDate.Normalize();
				if (importStartDate < GenBOEUtilities.AdjustDateTimePrecision((DateTime)inTaskElement.StartDate, DateTimePrecision.Month))
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange);
				}
				toReturn.StartDateValue = startDate.Normalize();
			}

			if (!importfromfile.ContainsKey(ImportExportConstants.END_DATE_COLUMN_HEADER) || !DateTime.TryParse(importfromfile[ImportExportConstants.END_DATE_COLUMN_HEADER], out endDate))
			{
				toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
			}
			else
			{
				importEndDate = endDate.Normalize();
				if (importEndDate > GenBOEUtilities.AdjustDateTimePrecision((DateTime)inTaskElement.EndDate, DateTimePrecision.Month))
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange);
				}
				toReturn.EndDateValue = endDate.Normalize();
			}

			if (!importfromfile.ContainsKey(ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER) || String.IsNullOrEmpty(importfromfile[ImportExportConstants.SPREAD_CURVE_COLUMN_HEADER]) || !spreadCurveSelection.HasValue)
			{
				toReturn.SpreadCurveID = SpreadCurves.DiscreteHours;
				toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
			}
			else
			{
				SpreadCurveModelView curve = (from spread in this.commonDataMapper.getSpreadCurve() where spread.SpreadCurveName == spreadCurveSelection.GetDescription() select spread).FirstOrDefault();
				if (curve != null)
				{
					toReturn.Curve = curve.SpreadCurveName;
					toReturn.SpreadCurveID = curve.SpreadCurveID;
				}
				else
				{
					toReturn.SpreadCurveID = SpreadCurves.DiscreteHours;
					toReturn.Curve = string.Empty;
					toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
				}
			}

			bool hasPercent = true;
			if (!importfromfile.ContainsKey(PERCENT_SPREAD_COL) || !decimal.TryParse(importfromfile[PERCENT_SPREAD_COL], out percentSpread))
			{
				hasPercent = false;
				toReturn.PercentSpread = null;
			}
			else
			{
				if (ImportUtils.IsMaxDecimalPlaces(percentSpread.ToString(), 6, out percentSpread))  // Percent Spread is always capped at 6 DP
				{
					toReturn.PercentSpread = percentSpread;
				}
				else  // round the percent spread down to 6 DP, but do not flag an error
				{
					toReturn.PercentSpread = decimal.Round(percentSpread, 6, MidpointRounding.AwayFromZero);
				}
			}

			bool hasHours = false;
			// Set the ValueSpread based on the spread type.
			if (toReturn.SpreadType == SpreadType.Hours)
			{
				// Get from the Hours Spread column.
				string hoursLabel = FullObjectHelper.ShowEquivalentPersonsOption && inWorkspace.IsUsingEquivalentPerson ? EP_SPREAD_COL : HOURS_SPREAD_COL;

				decimal hoursSpread = 0;
				if (!importfromfile.ContainsKey(hoursLabel) || !decimal.TryParse(importfromfile[hoursLabel], out hoursSpread))
				{
					toReturn.ValueSpread = null;
					if (!hasPercent && toReturn.SpreadCurveID != SpreadCurves.DiscreteHours)
					{
						// data is only missing if it's missing both % and hours. However, instead of throwing an error about missing data, default the values to 0
						hasHours = true;
						toReturn.ValueSpread = 0;
						toReturn.PercentSpread = 0;
					}
				}
				else
				{
					if (ImportUtils.IsMaxDecimalPlaces(hoursSpread.ToString(), inWorkspace.DecimalPrecision, out hoursSpread))
					{
						hasHours = true;
						toReturn.ValueSpread = hoursSpread;
					}
					else
					{
						toReturn.ImportTypes.Add(LaborTypeImportResult.HoursSpreadInvalid);
					}
				}
			}
			else if (toReturn.SpreadType == SpreadType.Cost)
			{
				// Get from the Cost column.
				decimal cost = 0;
				if (!importfromfile.ContainsKey(COST_COL) || !decimal.TryParse(importfromfile[COST_COL], NumberStyles.AllowCurrencySymbol | NumberStyles.Number, CultureInfo.CurrentCulture.NumberFormat, out cost))
				{
					// Ignore invalid Cost value for discrete cost. It does not come in to play.
					if (toReturn.SpreadCurveID != SpreadCurves.DiscreteCost)
					{
						// since the cost value could not be found or could not be parsed, set it to the default value
						toReturn.ValueSpread = 0;

						// if the column was located, it means the parsing failed. only throw an
						// error when the parsing fails. no column simply means the data in that
						// cell was cleard out which is valid.
						if (importfromfile.ContainsKey(COST_COL))
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.CostSpreadRangeInvalid);
						}
					}
				}
				else
				{
					if (ImportUtils.IsMaxDecimalPlacesForCost(cost.ToString(), inWorkspace.CostDecimalPrecision, out cost))
					{
						toReturn.ValueSpread = cost;
					}
					else
					{
						toReturn.ImportTypes.Add(LaborTypeImportResult.CostDecimalPrecisionViolation);
					}
				}
			}

			//add % Spread and Value Spread locked values
			if (toReturn.SpreadCurveID != SpreadCurves.DiscreteHours)
			{
				if (toReturn.PercentSpread.HasValue && (!hasHours || !toReturn.ValueSpread.HasValue))
				{
					toReturn.PercentSpreadLocked = true;
					toReturn.HourSpreadLocked = false;
				}
				else
				{
					toReturn.PercentSpreadLocked = false;
					toReturn.HourSpreadLocked = true;
				}
			}

			this.ImportCustomFields(toReturn, importfromfile, workspaceCustomFields, inWorkspace, ref newCustomFieldIndex);

			if (isMulti)
			{
				if (!importfromfile.ContainsKey(RESOURCE_CLIN_HEADER) && !importfromfile.ContainsKey(RESOURCE_WBS_HEADER))
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.ResourceMultiValuesInvalid);
				}
				else
				{
					if (importfromfile.ContainsKey(RESOURCE_WBS_HEADER))
					{
						toReturn.WBSID = -1;
						if (!string.IsNullOrEmpty(importfromfile[RESOURCE_WBS_HEADER]))
						{
							WbsDTO wbs = wsWbs.FirstOrDefault(w => w.WbsString == importfromfile[RESOURCE_WBS_HEADER]);
							if (wbs == null)
							{
								toReturn.ImportTypes.Add(LaborTypeImportResult.ResourceMultiValuesInvalid);
							}
							else
							{
								toReturn.WBSID = wbs.Id;
							}
						}
					}
					if (importfromfile.ContainsKey(RESOURCE_CLIN_HEADER))
					{
						toReturn.CLINID = -1;
						if (!string.IsNullOrEmpty(importfromfile[RESOURCE_CLIN_HEADER]))
						{
							ClinDTO clin = wsClins.FirstOrDefault(w => w.ClinString == importfromfile[RESOURCE_CLIN_HEADER]);
							if (clin == null)
							{
								toReturn.ImportTypes.Add(LaborTypeImportResult.ResourceMultiValuesInvalid);
							}
							else
							{
								toReturn.CLINID = clin.Id;
							}
						}
					}
				}
			}
			else
			{
				if (importfromfile.ContainsKey(RESOURCE_CLIN_HEADER) || importfromfile.ContainsKey(RESOURCE_WBS_HEADER))
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.ResourceMultiValuesInvalid);
				}
			}

			if (isOffload)
			{
				if (!importfromfile.ContainsKey(ImportExportConstants.OFFLOAD_COLUMN_HEADER))
				{
					toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
				}
				else
				{
					bool canOffload;
					if (bool.TryParse(importfromfile[ImportExportConstants.OFFLOAD_COLUMN_HEADER], out canOffload))
					{
						toReturn.CanOffload = canOffload;
					}
					else
					{
						toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
					}
				}
			}

			#endregion Labor Type

			NormalizeImportTypesResults(toReturn);

			#region Labor Spread

			// no invalid data, try to import the spread data now
			// only import for Discrete Cost/Hours
			if (!toReturn.ImportTypes.Any() && (spreadCurveSelection == SpreadCurves.DiscreteCost || spreadCurveSelection == SpreadCurves.DiscreteHours))
			{
				// derive complete spread date range for this resource
				ICollection<string> resourceSpreadDateRange = new List<string>();
				for (DateTime dt = importStartDate.Value; dt <= importEndDate; dt = dt.AddMonths(1))
				{
					resourceSpreadDateRange.Add(dt.ToString("MM/yyyy"));
				}

				// BOEJ-3307 - if any spread month values are outside the spread date range, do not import.
				foreach (KeyValuePair<string, string> entry in importfromfile)
				{
					DateTime spreadMonthDate;
					if (DateTime.TryParseExact(entry.Key, "MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out spreadMonthDate))
					{
						if (!resourceSpreadDateRange.Contains(entry.Key) && !string.IsNullOrWhiteSpace(entry.Value))
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.SpreadMonthValueOutsideDateRange);
							return toReturn;
						}
					}
				}

				// detect any missing spread date/value entries and create a zero-value entry for each
				var dateCheckResults =
					(from d in resourceSpreadDateRange
					 join a in importfromfile.Keys on d equals a into g
					 from k in g.DefaultIfEmpty()
					 select new { SpreadDate = d, Match = k }).ToList();

				foreach (string missingSpreadDate in dateCheckResults.Where(r => r.Match == null).Select(m => m.SpreadDate))
				{
					importfromfile.Add(missingSpreadDate, "0");
				}

				foreach (string spreadMonth in resourceSpreadDateRange)
				{
					string importedSpreadMonthValue;
					importfromfile.TryGetValue(spreadMonth, out importedSpreadMonthValue);
					DateTime spreadDate;
					if (DateTime.TryParseExact(spreadMonth, "MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out spreadDate))
					{
						// "normalize" spread date
						spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spreadDate, DateTimePrecision.Month);

						decimal SpreadValue = 0;

						LaborSpreadImportResult reasonInvalid = LaborSpreadImportResult.InvalidSpreadValue;

						bool isInvalidSpreadValue = false;

						if (string.IsNullOrWhiteSpace(importedSpreadMonthValue))
						{
							SpreadValue = 0;
						}
						else if (toReturn.SpreadType == SpreadType.Cost)
						{
							decimal costSpreadValue = 0;

							bool decimalPlacesGood = ImportUtils.IsMaxDecimalPlacesForCost(importedSpreadMonthValue, inWorkspace.CostDecimalPrecision, out costSpreadValue);
							if (decimalPlacesGood)
							{
								SpreadValue = costSpreadValue;
								if (!ImportUtils.IsSpread12Digits((long)SpreadValue))
								{
									isInvalidSpreadValue = true;
									reasonInvalid = LaborSpreadImportResult.ResourceCostValueTooLarge;
								}
							}
							else
							{
								isInvalidSpreadValue = true;
								reasonInvalid = LaborSpreadImportResult.CostDecimalPrecisionViolation;
							}
						}
						else
						{
							// Validate hours spread value.
							if (!ImportUtils.IsMaxDecimalPlaces(importedSpreadMonthValue, inWorkspace.DecimalPrecision, out SpreadValue))
							{
								SpreadValue = 0;
								isInvalidSpreadValue = true;
								reasonInvalid = LaborSpreadImportResult.DecimalPrecisionViolation;
							}
							else if (!ImportUtils.IsSpread10Digits((long)SpreadValue))
							{
								isInvalidSpreadValue = true;
								reasonInvalid = LaborSpreadImportResult.ResourceHourValueTooLarge;
							}
						}

						// is there an existing entry for this spread date/value?
						DateTime storedLaborSpreadDate = new DateTime(spreadDate.Year, spreadDate.Month, 15);

						ResourceSpreadDto existingResourceSpread = (existingResource != null) ? existingResource.LaborSpreads.SingleOrDefault(s => s.LaborSpreadDate.Date == storedLaborSpreadDate.Date) : null;
						if (existingResourceSpread == null)
						{
							existingResourceSpread = new ResourceSpreadDto
							{
								Id = -1,
								LaborSpreadDate = spreadDate,
								LaborSpreadValue = SpreadValue,
								Updateable = UpdateType.Upsert
							};
						}

						// Spread value given was invalid
						if (isInvalidSpreadValue)
						{
							ImportedLaborSpread updatedLaborSpread = new ImportedLaborSpread(existingResourceSpread, existingResource, this.resourceDTODataLoader, this.perfOrgLoader, reasonInvalid);
							updatedLaborSpread.LaborSpreadValue = SpreadValue;

							toReturn.ImportedLaborSpreads.Add(updatedLaborSpread);
						}
						// If this is an existing spread with an updated value
						else if ((existingResourceSpread.Id < 0 || existingResourceSpread.LaborSpreadValue != SpreadValue))
						{
							ImportedLaborSpread updatedLaborSpread = new ImportedLaborSpread(existingResourceSpread, existingResource, this.resourceDTODataLoader, this.perfOrgLoader, LaborSpreadImportResult.UpdateSpread);
							updatedLaborSpread.LaborSpreadValue = SpreadValue;

							toReturn.ImportedLaborSpreads.Add(updatedLaborSpread);
						}
					}
				}

				Collection<ResourceSpreadDto> originalLaborSpreads = (existingResource != null) ? existingResource.LaborSpreads : new Collection<ResourceSpreadDto>();
				VerifyTotalSumOfResource(toReturn.ImportedLaborSpreads, originalLaborSpreads, toReturn.SpreadType);
			}

			#endregion Labor Spread

			return toReturn;
		}

		/// <summary>
		/// Cleans up the data to make sure that we are not sending the same error more than once, based on how things are counted.
		/// This prevents wrong records being displayed on the Import page
		/// </summary>
		internal static void NormalizeImportTypesResults(ImportedLaborType toReturn)
		{
			if (toReturn == null) { return; }

			// remove any duplicates
			toReturn.ImportTypes = toReturn.ImportTypes.Distinct().ToCollection();

			// For labor types, the following errors are grouped together, and counted as if they were the same thing:
			List<LaborTypeImportResult> equivalentLaborTypeErrors = new List<LaborTypeImportResult>() { LaborTypeImportResult.MissingData, LaborTypeImportResult.ResourceMultiValuesInvalid, LaborTypeImportResult.InvalidData };

			while (toReturn.ImportTypes.Count(x => equivalentLaborTypeErrors.Contains(x)) > 1)
			{
				toReturn.ImportTypes.Remove(toReturn.ImportTypes.First(x => equivalentLaborTypeErrors.Contains(x)));
			}

			// A second group of labor type errors:
			equivalentLaborTypeErrors = new List<LaborTypeImportResult>() { LaborTypeImportResult.CostDecimalPrecisionViolation, LaborTypeImportResult.CostSpreadRangeInvalid };

			while (toReturn.ImportTypes.Count(x => equivalentLaborTypeErrors.Contains(x)) > 1)
			{
				toReturn.ImportTypes.Remove(toReturn.ImportTypes.First(x => equivalentLaborTypeErrors.Contains(x)));
			}
		}

		/// <summary>
		/// Imports the custom fields from the row.
		/// </summary>
		/// <param name="toReturn">The labor type resource to return.</param>
		/// <param name="importfromfile">The imported row data.</param>
		/// <param name="workspaceCustomFields">The workspace custom fields.</param>
		/// <param name="inWorkspace">The workspace.</param>
		/// <param name="newCustomFieldIndex">int keeping track of negative ids for new Custom Fields</param>
		private void ImportCustomFields(ImportedLaborType toReturn, Dictionary<string, string> importfromfile, ICollection<CustomFieldDTO> workspaceCustomFields, FullWorkspace inWorkspace, ref int newCustomFieldIndex)
		{
			if (workspaceCustomFields.Any())
			{
				Dictionary<int, ICollection<KeyValuePair<int, int>>> mappings = inWorkspace.LaborTypesMappingWithCustomFieldsValuesAndContainerIds;
				ICollection<KeyValuePair<int, int>> mapping;

				mappings.TryGetValue(toReturn.Id, out mapping);

				toReturn.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();

				foreach (CustomFieldDTO customField in workspaceCustomFields)
				{
					string customFieldKey = ExcelUtilities.SetPrefixCustomField(customField.CustomFieldName);
					// Check to make sure required customFields are set
					if (customField.CustomFieldRequired)
					{
						customFieldKey = ExcelUtilities.SetPrefixCustomFieldRequired(customField.CustomFieldName);
						if (!importfromfile.ContainsKey(customFieldKey) || string.IsNullOrWhiteSpace(importfromfile[customFieldKey]))
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.MissingData);
						}
					}

					ICollection<int> valueIds = inWorkspace.CustomFieldValues.Where(cf => cf.CustomFieldID == customField.Id).Select(cfv => cfv.CustomFieldValueID).ToList();
					if (importfromfile.ContainsKey(customFieldKey) && !string.IsNullOrWhiteSpace(importfromfile[customFieldKey]))
					{

						/*
                            * 
                            * 
                            * SelectionID      CustomFieldID                       CustomFieldValueID
                            * (BLTCFVID)       (the custom field variable)         (drop-down value)
                            * -------------    --------------------------------    -----------------------------
                            * 44775            7102 (ResrcCustomFieldOne)          -1
                            * 44776            7105 (ResrcCustomFieldTwo)          11049 (99-ResourceCFValThree)
                            * 
                            * 
                            *                          CustomFieldValueID
                            *                      +--------------+--------------+
                            *                      |      -1      |      999     |
                            *              +-------+--------------+--------------+
                            *              |       | new row w/   | new row      |
                            *              |  -1   | unselected   | w/ value     |
                            *              |       | value        | assigned     |
                            *              |       | => SKIP      | => UPSERT    |
                            * SelectionID  |-------+--------------+--------------+
                            *              |       | existing     | existing     |
                            *              |  999  | row w/ value | row w/ value |
                            *              |       | DE-selected  | assigned     |
                            *              |       | => DELETE    | => UPSERT    |
                            *              +-------+--------------+--------------+
                            * 
                            */

						int selectionID;
						if (mapping == null || !mapping.Any(kvp => valueIds.Contains(kvp.Value)))
						{
							// There are no mappings for this custom field, so it must be new
							selectionID = -1;
						}
						else
						{
							KeyValuePair<int, int> containerValuePair = mapping.First(kvp => valueIds.Contains(kvp.Value));
							selectionID = containerValuePair.Key;
						}

						// Process the Custom Field value
						string value = importfromfile[customFieldKey];
						bool successfulParsing = false;
						UpdateType typeOfUpdate = UpdateType.None;
						string cfvDescription = string.Empty;

						ResourceTypeDto resourceType = inWorkspace.TaskElements.SelectMany(x => x.taskElementLabors).FirstOrDefault(x => x.Id == toReturn.Id);
						CustomFieldValueContainer customFieldValue = resourceType != null ? resourceType.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id) : null;
						int customFieldValueID = customFieldValue == null ? newCustomFieldIndex-- : customFieldValue.CustomFieldValueID;

						if (customField.IsOpenEnded)
						{
							// Handle open ended CFs
							// Value is in the form of {0} CustomFieldValueDescription.
							cfvDescription = value = value.Trim();
							typeOfUpdate = !string.IsNullOrEmpty(value) ? UpdateType.Upsert : ((selectionID > 0) ? UpdateType.Deleted : UpdateType.None);
							successfulParsing = true;
						}
						else
						{
							if (string.IsNullOrWhiteSpace(value))
							{
								// the new value is empty
								successfulParsing = true;
								if (customFieldValueID > 0)
								{
									typeOfUpdate = UpdateType.Deleted;
								}
								else
								{
									// nothing changed, original and new value are emtpy
									typeOfUpdate = UpdateType.None;
								}
							}
							else if (value.Contains('-'))
							{
								// Get custom field value description in format {ID} - {Description}
								string nameAndDescription = value;

								// match the selected Name/Description with the allowed values
								CustomFieldValueDTO customFieldValueSelected = inWorkspace.CustomFieldValues.FirstOrDefault(cf => cf.CustomFieldID == customField.Id && nameAndDescription == string.Format("{0} - {1}", cf.CustomFieldValueName, cf.CustomFieldValueDescription));

								if (customFieldValueSelected == null)
								{
									// the row was input with a bad value
									successfulParsing = false;
								}
								else if (customFieldValueSelected.Id != customFieldValueID)
								{
									// original value was changed
									customFieldValueID = customFieldValueSelected.Id;
									typeOfUpdate = UpdateType.Upsert;
									successfulParsing = true;
								}
								else if (customFieldValueSelected.Id == customFieldValueID)
								{
									// original value was not changed
									typeOfUpdate = UpdateType.None;
									successfulParsing = true;
								}
							}
						}

						if (successfulParsing)
						{
							if (typeOfUpdate != UpdateType.None)
							{
								toReturn.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
								{
									Id = selectionID < 0 ? newCustomFieldIndex-- : selectionID,
									CustomFieldValueID = customFieldValueID,
									Updateable = typeOfUpdate,
									ContainerID = selectionID < 0 ? newCustomFieldIndex-- : selectionID,
									CustomFieldID = customField.Id,
									IsOpenEnded = customField.IsOpenEnded,
									OpenEndedValue = cfvDescription,
									// IES-665 - ensures database update time will not be less than creation
									UpdateDate = DateTime.MinValue
								});
							}
						}
						else
						{
							toReturn.ImportTypes.Add(LaborTypeImportResult.InvalidData);
						}
					}
					else if (mapping != null)
					{
						// There was an empty value for this custom field in a row that is not new, track down its mapping through the custom field values and set to delete if found
						if (mapping.Any(kvp => valueIds.Contains(kvp.Value)))
						{
							KeyValuePair<int, int> containerValuePair = mapping.First(kvp => valueIds.Contains(kvp.Value));
							toReturn.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
							{
								Id = containerValuePair.Key,
								CustomFieldValueID = -1,
								Updateable = UpdateType.Deleted,
								ContainerID = containerValuePair.Key,
								UpdateDate = DateTime.Now
							});
						}
					}
				} // end foreach Custom Field
			}
		}

		#endregion Private Functions
	}

	public enum LaborTypeImportResult
	{
		UpdateLaborType = 0,
		AddLaborType = 1,
		MissingData = 2,
		SpreadCurveChangeNeed = 3,
		InvalidData = 4,
		LaborTypeDateOutsideOfPOPDateRange = 5,
		UnChanged = 6,
		ElementOfCostMissingInvalid = 7,  // Never actually used except in if blocks (should be removed?)
		ResourceMissingInvalid = 8, // only used in Work Offline Importer
		PerfOrgMissingInvalid = 9,  // only used in Work Offline Importer
		StartDateMissingInvalid = 10, // only used in Work Offline Importer
		EndDateMissingInvalid = 11, // only used in Work Offline Importer
		SpreadCurveMissing = 12, // only used in Work Offline Importer
		SpreadCurveInvalid = 13, // only used in Work Offline Importer
		NonUniqueResourceTypeID = 14, // only used in Work Offline Importer
		ResourceTypeIDMissingOrInvalid = 15,
		RateTypeSpreadTypeAgreement = 16,
		LaborTypeDateSpreadDateMismatch = 17, // only used in Work Offline Importer
		HoursSpreadInvalid = 18,
		DecimalPrecisionViolation = 19, // only used in Work Offline Importer
		CostSpreadRangeInvalid = 20,
		CostDecimalPrecisionViolation = 21,
		ResourceSpreadsInvalid = 22, // only used in Work Offline Importer
		ResourceMultiValuesInvalid = 23,
		MissingRequiredResourceCustomField = 24,
		SpreadMonthColumnInvalid = 25,
		SpreadMonthValueOutsideDateRange = 26
	}

	[ExcludeFromCodeCoverage]
	public class ImportedLaborType : ResourceTypeDto
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedLaborType"/> class.
		/// </summary>
		public ImportedLaborType()
		{
			this.ImportTypes = new Collection<LaborTypeImportResult>();
			this.ImportedLaborSpreads = new Collection<ImportedLaborSpread>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedLaborType"/> class.
		/// </summary>
		/// <param name="inBOELaborType">The Labor Type resource.</param>
		public ImportedLaborType(ResourceTypeDto inBOELaborType)
			: base(inBOELaborType)
		{
			this.ImportTypes = new Collection<LaborTypeImportResult>();
			this.ImportedLaborSpreads = new Collection<ImportedLaborSpread>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedLaborType"/> class.
		/// </summary>
		/// <param name="inBOELaborType">The Labor Type resource.</param>
		/// <param name="inImportResult">The import result.</param>
		public ImportedLaborType(ResourceTypeDto inBOELaborType,
		   LaborTypeImportResult inImportResult)
			: this(inBOELaborType)
		{
			this.ImportTypes.Add(inImportResult);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportedLaborType"/> class.
		/// </summary>
		/// <param name="inBOELaborType">The Labor Type resource.</param>
		/// <param name="inResourceDTODataLoader">The resource dto data loader.</param>
		/// <param name="perfOrgLoader">The performing org loader.</param>
		/// <param name="inImportResult">The import result.</param>
		/// <exception cref="System.ArgumentNullException">
		/// inResourceDTODataLoader or perfOrgLoader</exception>
		public ImportedLaborType(
			ResourceTypeDto inBOELaborType,
			IResourceDTODataLoader inResourceDTODataLoader,
			IPerformingOrgDTODataLoader perfOrgLoader,
			LaborTypeImportResult inImportResult)
			: this(inBOELaborType, inImportResult)
		{

			if (inResourceDTODataLoader == null)
			{
				throw new ArgumentNullException(nameof(inResourceDTODataLoader));
			}

			if (perfOrgLoader == null)
			{
				throw new ArgumentNullException(nameof(perfOrgLoader));
			}

			if (inBOELaborType != null)
			{
				this.LaborTypeID = inBOELaborType.Id;

				if (inBOELaborType.ResourceID.HasValue)
				{
					this.Resource = inResourceDTODataLoader.GetById(inBOELaborType.ResourceID.Value).ResourceName;
				}

				if (inBOELaborType.BusinessResourceCodeID.HasValue)
				{
					this.BusinessResourceCode = inResourceDTODataLoader.GetById(inBOELaborType.ResourceID.Value).ResourceName;
				}

				if (inBOELaborType.PerformingOrgID.HasValue)
				{
					this.PerformingOrg = perfOrgLoader.GetById(inBOELaborType.PerformingOrgID.Value).PerformingOrgName;
				}
			}
		}

		public string StartDateFormatted
		{
			get
			{
				string toReturn = string.Empty;

				if (this.StartDate.HasValue)
				{
					toReturn = this.StartDate.Value.ToString("MM/yyyy");
				}

				return toReturn;
			}
		}

		public string EndDateFormatted
		{
			get
			{
				string toReturn = string.Empty;

				if (this.EndDate.HasValue)
				{
					toReturn = this.EndDate.Value.ToString("MM/yyyy");
				}

				return toReturn;
			}
		}

		public int SpreadCurveIDIntValue
		{
			get
			{
				return (int)this.SpreadCurveID;
			}
			set
			{
				this.SpreadCurveID = (SpreadCurves)value;
			}
		}

		public int SpreadTypeIDIntValue
		{
			get
			{
				return (int)this.SpreadType;
			}
			set
			{
				this.SpreadType = (SpreadType)value;
			}
		}

		/// <summary>
		/// A collection of types that match this row.  Combinations
		/// of some items can be in place (Invalid, Missing, etc)
		/// but only 1 of Added, Updated, Unchanged should be in place.
		/// </summary>
		public Collection<LaborTypeImportResult> ImportTypes { get; set; }


		/// <summary>
		/// Gets or sets the imported labor spreads.
		/// </summary>
		public Collection<ImportedLaborSpread> ImportedLaborSpreads { get; set; }

		public Collection<int> ImportTypesIntValues
		{
			get
			{
				Collection<int> toGet = new Collection<int>();
				foreach (LaborTypeImportResult intvalue in this.ImportTypes)
				{
					toGet.Add((int)intvalue);
				}
				return toGet;
			}
			set
			{
				if (value != null)
				{
					this.ImportTypes.Clear();

					foreach (int intvalue in value)
					{
						this.ImportTypes.Add((LaborTypeImportResult)intvalue);
					}
				}
			}
		}

		public int? LaborTypeID { get; set; }

		public string Resource { get; set; }
		public string BusinessResourceCode { get; set; }
		public string Wbs { get; set; }
		public string Clin { get; set; }
		public string PerformingOrg { get; set; }
		public string Curve { get; set; }
	}
}