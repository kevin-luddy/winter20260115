namespace GenBOE.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// Skill Mix Calculation
	/// </summary>
	public class SkillMixUtility
	{
		/// <summary>
		/// Refreshes the Skill Mix Tables with updated resource hours
		/// </summary>
		/// <param name="laborTypes">The labor type/spreads data</param>
		/// <param name="currentCommonDisclosureData">Current Common Disclosure data</param>
		/// <param name="resourceHours">MOQ Table Resource Hours</param>
		/// <param name="currentSkillMixData">The current skill mix data</param>
		/// <param name="currentSkillMixSummaryData">The current skill mix summary data</param>
		/// <param name="isBRCEnabled">Is BRC Enabled for CD row check.</param>
		/// <param name="isManual">If the Historical Resource/Hours are Manually input or not</param>
		/// <returns>Refreshed/Recalculated Skill Mix Model View</returns>
		public static RefreshSkillMixModelView RefreshSkillMixTables(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
			ICollection<CommonDisclosureModelView> currentCommonDisclosureData, ICollection<SkillMixSummaryModelView> currentSkillMixSummaryData, bool isBRCEnabled, bool isManual, DateTime? taskEndDate)
		{
			// null checks 
			if (resourceHours == null)
			{
				resourceHours = new List<MOQTypeSelectionTableDataResourceHoursDTO>();
			}

			if (currentSkillMixData == null)
			{
				currentSkillMixData = new List<SkillMixModelView>();
			}

			if (currentCommonDisclosureData == null)
			{
				currentCommonDisclosureData = new List<CommonDisclosureModelView>();
			}

			if (currentSkillMixSummaryData == null)
			{
				currentSkillMixSummaryData = new List<SkillMixSummaryModelView>();
			}

			// Only do Common Disclosure if the task is past the 1LMX start date
			isBRCEnabled = isBRCEnabled && taskEndDate.HasValue && taskEndDate >= Utilities.OneLmxStartDate;

			RefreshSkillMixModelView refreshedModel;
			laborTypes = laborTypes == null ? new List<LaborTypeDataModelView>() : laborTypes.Where(l => l.RateType == RateType.Hours).ToList();

			if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.SpaceSystems)
			{
				refreshedModel = RefreshSpaceSkillMix(resourceHours, laborTypes, currentSkillMixSummaryData);
			}
			else
			{
				// RMS
				refreshedModel = RefreshRMSSkillMix(resourceHours, laborTypes, currentSkillMixData, currentCommonDisclosureData, isBRCEnabled, isManual);
				CleanupData(refreshedModel);
			}
						
			CalculateSkillMixTotals(refreshedModel);
			CalculateBoeSkillMixPercentage(refreshedModel);

            // Apply rationale canned responses based on conditions regarding proposed skill mix and historical hours.
            ApplyRationaleCannedResponses(refreshedModel);

            // reorder the lists
            refreshedModel.SkillMixRows = refreshedModel.SkillMixRows.OrderBy(r => string.IsNullOrWhiteSpace(r.ResourceOld)).ThenBy(r => r.ResourceOld).ToList();
			refreshedModel.CommonDisclosureRows = refreshedModel.CommonDisclosureRows.OrderBy(r => string.IsNullOrWhiteSpace(r.ResourceID)).ThenBy(r => r.ResourceID).ThenBy(s => s.BusinessResourceID).ToList();
			refreshedModel.SkillMixSummaryRows = refreshedModel.SkillMixSummaryRows.OrderBy(r => string.IsNullOrWhiteSpace(r.ResourceID)).ThenBy(r => r.ResourceID).ThenBy(s => s.BusinessResourceID).ToList();
			
			return refreshedModel;
		}

		/// <summary>
		/// Refresh Skill Mix for RMS
		/// </summary>
		/// <param name="laborTypes">The labor type/spreads data</param>
		/// <param name="currentCommonDisclosureData">Current Common Disclosure data</param>
		/// <param name="resourceHours">MOQ Table Resource Hours</param>
		/// <param name="currentSkillMixData">The current skill mix data</param>
		/// <param name="isBRCEnabled">Is BRC Enabled for CD row check.</param>
		/// <param name="isManual">If the Historical Resource/Hours are Manually input or not</param>
		/// <returns>Refreshed/Recalculated Skill Mix Model View</returns>
		private static RefreshSkillMixModelView RefreshRMSSkillMix(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData, ICollection<CommonDisclosureModelView> currentCommonDisclosureData, bool isBRCEnabled, bool isManual)
		{
			RefreshSkillMixModelView refreshedModel = new RefreshSkillMixModelView();

			if (isManual)
			{
				// Mock out the Resource Hours from input data
				resourceHours = MockResourceHours(currentSkillMixData);
			}

			if (resourceHours.Any())
			{
				bool addBlankRow = true;

				// filter out bad data in currentSkillMixData
				FilterBadDataRMS(laborTypes, currentSkillMixData, currentCommonDisclosureData, resourceHours, isManual);

				if (isManual)
				{
					refreshedModel.SkillMixRows.AddRange(currentSkillMixData);
					addBlankRow = !currentSkillMixData.Any(s => string.IsNullOrWhiteSpace(s.ResourceOld));
				}
				else
				{
					ICollection<IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO>> groupedResourceHours = resourceHours.GroupBy(r => r.ResourceName).OrderBy(t => t.Key).ToList();
					foreach (IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO> grouping in groupedResourceHours)
					{
						decimal totalGroupHours = grouping.Sum(g => g.TotalHours);

						refreshedModel.SkillMixRows.Add(
							new SkillMixModelView
							{
								HistoricalHours = totalGroupHours,
								ResourceOld = grouping.Key,
								ResourceNew = string.Empty,
								Included = false
							}
						);

						if (string.IsNullOrWhiteSpace(grouping.Key))
						{
							addBlankRow = false;
						}
					}
				}

				if (addBlankRow)
				{
					// Always add a row with no historical/legacy resource set
					// This row is used to add row(s) information about Resources and BRCs that were used but are NOT tied to historical/legacy resources
					// This can be the case if the MOQ Type was not Historical/Comparative but user still wanted to use that MOQ Type as a reference
					refreshedModel.SkillMixRows.Add(
					new SkillMixModelView
					{
						HistoricalHours = 0m,
						ResourceOld = string.Empty,
						ResourceNew = string.Empty,
						LaborSkillMix = 0m,
						Included = false
					});
					addBlankRow = false;
				}

				CopyMatchingSkillMixRowDataRMS(resourceHours, laborTypes, currentSkillMixData, refreshedModel, isBRCEnabled, isManual);
				CleanupNewSkillMixRow(currentSkillMixData);
				

				if (isBRCEnabled)
				{
					CreateCommonDisclosureRowsRMS(resourceHours, laborTypes, currentCommonDisclosureData,
								refreshedModel, isManual);
					CleanUpCommonDisclosureRows(currentSkillMixData, refreshedModel);
				}
				else
				{
					refreshedModel.CommonDisclosureRows?.Clear();
				}
			}
			else
			{
				// Add the blank row
				refreshedModel.SkillMixRows.Add(
				new SkillMixModelView
				{
					HistoricalHours = 0m,
					ResourceOld = string.Empty,
					LaborSkillMix = 0m,
					Included = false
				});
			}

			return refreshedModel;
		}

		/// <summary>
		/// Refresh Skill Mix for Space
		/// </summary>
		/// <param name="laborTypes">The labor type/spreads data</param>
		/// <param name="resourceHours">MOQ Table Resource Hours</param>
		/// <param name="currentSkillMixSummaryData">The current skill mix summary data</param>
		/// <returns>Refreshed/Recalculated Skill Mix Model View</returns>
		private static RefreshSkillMixModelView RefreshSpaceSkillMix(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixSummaryModelView> currentSkillMixSummaryData)
		{
			List<SkillMixSummaryModelView> newSummaryRows = new List<SkillMixSummaryModelView>();

			// Add missing Labor Types
			AddMissingSpaceResources(laborTypes, newSummaryRows);
			
			// Add missing Historical Resources
			AddMissingSpaceHistoricalResources(resourceHours, newSummaryRows);
						
			// Copy over Rationale
			AddRationale(currentSkillMixSummaryData, newSummaryRows);

			RefreshSkillMixModelView refreshSkillMixModel = new RefreshSkillMixModelView();
			refreshSkillMixModel.SkillMixSummaryRows = newSummaryRows;

			return refreshSkillMixModel;
		}

		/// <summary>
		/// Adds rationale from old rows to new rows
		/// </summary>
		/// <param name="currentSkillMixSummaryData">The old summary rows</param>
		/// <param name="newSummaryRows">The new summary rows</param>
		private static void AddRationale(ICollection<SkillMixSummaryModelView> currentSkillMixSummaryData, List<SkillMixSummaryModelView> newSummaryRows)
		{
			foreach (SkillMixSummaryModelView newRow in newSummaryRows)
			{
				SkillMixSummaryModelView oldRow = currentSkillMixSummaryData.FirstOrDefault(s => s.ResourceID.NullEmptyEquals(newRow.ResourceID) &&
				s.BusinessResourceID.NullEmptyEquals(newRow.BusinessResourceID));
				if (oldRow != null)
				{
					newRow.Rationale = oldRow.Rationale;
				}
			}
		}

		/// <summary>
		/// Add missing Labor Resources for Space
		/// </summary>
		/// <param name="laborTypes">The labor type/spreads data</param>
		/// <param name="currentSkillMixSummaryData">The current skill mix summary data</param>
		private static void AddMissingSpaceResources(ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixSummaryModelView> currentSkillMixSummaryData)
		{
			// group the labor types together where Resource/BRC both match
			Dictionary<Tuple<string, string>, ICollection<LaborTypeDataModelView>> groupedLaborTypes = new Dictionary<Tuple<string, string>, ICollection<LaborTypeDataModelView>>();
			foreach (LaborTypeDataModelView labor in laborTypes)
			{
				if (string.IsNullOrEmpty(labor.ResourceName) && string.IsNullOrEmpty(labor.BusinessResourceCodeName))
				{
					// skip this row, no resource set
					continue;
				}

				Tuple<string, string> key = groupedLaborTypes.Keys.FirstOrDefault(g => g.Item1.NullEmptyEquals(labor.ResourceName) && g.Item2.NullEmptyEquals(labor.BusinessResourceCodeName));
				if (key == null)
				{
					key = new Tuple<string, string>(labor.ResourceName, labor.BusinessResourceCodeName);
					groupedLaborTypes.Add(key, new List<LaborTypeDataModelView>());
				}

				groupedLaborTypes[key].Add(labor);
			}

			// compare the labor type groups against the skill mix summary
			foreach (KeyValuePair<Tuple<string, string>, ICollection<LaborTypeDataModelView>> kvp in groupedLaborTypes)
			{
				List<LaborSpreadDataModelView> allSpreads = kvp.Value.SelectMany(r => r.Spreads).ToList();
				decimal totalHours = allSpreads.Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) < Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
				decimal totalHoursBRCs = allSpreads.Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) >= Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
				decimal totalUCOTHours = kvp.Value.SelectMany(r => r.UcotSpreads ?? new List<LaborSpreadDataModelView>()).Sum(x => x.LaborSpreadValue ?? 0m);

				SkillMixSummaryModelView summary = currentSkillMixSummaryData.FirstOrDefault(s => s.ResourceID.NullEmptyEquals(kvp.Key.Item1) && s.BusinessResourceID.NullEmptyEquals(kvp.Key.Item2));
				if (summary == null)
				{
					summary = new SkillMixSummaryModelView()
					{
						ResourceID = kvp.Key.Item1,
						BusinessResourceID = kvp.Key.Item2
					};
					currentSkillMixSummaryData.Add(summary);
				}

				// update the historical hours from the Resource Hours calculated in SAP
				summary.ProposedLegacyResource = totalHours;
				summary.ProposedBrc = totalHoursBRCs;
				summary.UCOTHours = totalUCOTHours;
				summary.Included = true;
			}
		}

		/// <summary>
		/// Add missing Historical Resources for Space
		/// </summary>
		/// <param name="resourceHours">MOQ Table Resource Hours</param>
		/// <param name="currentSkillMixSummaryData">The current skill mix summary data</param>
		private static void AddMissingSpaceHistoricalResources(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, ICollection<SkillMixSummaryModelView> currentSkillMixSummaryData)
		{
			// group the historical together where Resource/BRC both match
			Dictionary<Tuple<string, string>, ICollection<MOQTypeSelectionTableDataResourceHoursDTO>> groupedHistorical = new Dictionary<Tuple<string, string>, ICollection<MOQTypeSelectionTableDataResourceHoursDTO>>();
			foreach (MOQTypeSelectionTableDataResourceHoursDTO historical in resourceHours)
			{
				if (string.IsNullOrEmpty(historical.ResourceName) && string.IsNullOrEmpty(historical.BRCName))
				{
					// skip this row, no resources set
					continue;
				}

				Tuple<string, string> key = groupedHistorical.Keys.FirstOrDefault(g => g.Item1.NullEmptyEquals(historical.ResourceName) && g.Item2.NullEmptyEquals(historical.BRCName));
				if (key == null)
				{
					key = new Tuple<string, string>(historical.ResourceName, historical.BRCName);
					groupedHistorical.Add(key, new List<MOQTypeSelectionTableDataResourceHoursDTO>());
				}

				groupedHistorical[key].Add(historical);
			}

			// compare the historical groups against the skill mix summary
			foreach (KeyValuePair<Tuple<string, string>, ICollection<MOQTypeSelectionTableDataResourceHoursDTO>> kvp in groupedHistorical)
			{
				decimal totalHistoricalHours = kvp.Value.Sum(r => r.TotalHours);
				
				SkillMixSummaryModelView summary = currentSkillMixSummaryData.FirstOrDefault(s => s.ResourceID.NullEmptyEquals(kvp.Key.Item1) && s.BusinessResourceID.NullEmptyEquals(kvp.Key.Item2));
				if (summary == null)
				{
					summary = new SkillMixSummaryModelView()
					{
						ResourceID = kvp.Key.Item1,
						BusinessResourceID = kvp.Key.Item2
					};
					currentSkillMixSummaryData.Add(summary);
				}

				// update the historical hours from the Resource Hours calculated in SAP
				summary.HistoricalHours = totalHistoricalHours;
			}
		}

		/// <summary>
		/// Set historical hours to zero for added rows
		/// </summary>
		/// <param name="currentSkillMixData">Collection of skillmix data</param>
		private static void CleanupNewSkillMixRow(ICollection<SkillMixModelView> currentSkillMixData)
		{
			List<string> resourceOldList = new List<string>();
			currentSkillMixData = currentSkillMixData.OrderBy(sm => sm.ResourceOld).ThenBy(sm => string.IsNullOrWhiteSpace(sm.ResourceNew) ? 1 : 0).ToArray();
			foreach (SkillMixModelView item in currentSkillMixData)
			{
				if(resourceOldList.Contains(item.ResourceOld))
				{
					item.HistoricalHours = 0;
				}

				resourceOldList.Add(item.ResourceOld);
			}
		}

		/// <summary>
		/// Clean up common disclosure rows after adding new rows in skill mix table
		/// </summary>
		/// <param name="currentSkillMixData">Collection of skillmix data</param>
		/// <param name="refreshedModel">The Refreshed Skill Mix Model</param>
		private static void CleanUpCommonDisclosureRows(ICollection<SkillMixModelView> currentSkillMixData, RefreshSkillMixModelView refreshedModel)
		{
			List<string> resourceIdList = new List<string>();
			ICollection<string> newLinkedResourceIds = refreshedModel.SkillMixRows.Where(r => !string.IsNullOrWhiteSpace(r.ResourceNew)).Select(l => l.ResourceNew).Distinct().ToList();
			foreach (CommonDisclosureModelView item in refreshedModel.CommonDisclosureRows)
			{
				if (newLinkedResourceIds.Contains(item.ResourceID))
				{
					item.HistoricalHours = currentSkillMixData.Where(sm => item.ResourceID == sm.ResourceNew).Sum(l => l.HistoricalHours);
				} else
				{
					IEnumerable<SkillMixModelView> currentData = currentSkillMixData.Where(sm => item.ResourceID == sm.ResourceOld && string.IsNullOrWhiteSpace(sm.ResourceNew));
					item.HistoricalHours = currentData.Any() ? currentData.Select(l => l.HistoricalHours).First() : item.HistoricalHours;
				}

				if (resourceIdList.Contains(item.ResourceID))
				{
					item.HistoricalHours = 0;
				}

				resourceIdList.Add(item.ResourceID);
			}

		}

		/// <summary>
		/// Create the Common Disclosure Rows from the data
		/// </summary>
		/// <param name="resourceHours">The resource hours</param>
		/// <param name="laborTypes">labor type data</param>
		/// <param name="currentCommonDisclosureData">Current Common Disclosure Data</param>
		/// <param name="refreshedModel">The Refreshed Skill Mix Model</param>
		/// <param name="isManual">Is this Manual or Automated SkillMix</param>
		protected static void CreateCommonDisclosureRowsRMS(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<CommonDisclosureModelView> currentCommonDisclosureData,
			RefreshSkillMixModelView refreshedModel, bool isManual)
		{
			if (laborTypes == null)
			{
				throw new ArgumentNullException(nameof(laborTypes));
			}

			if (refreshedModel == null)
			{
				throw new ArgumentNullException(nameof(refreshedModel));
			}

			// Create Common Disclosure Rows by taking the list of Resources assigned in Skill Mix table, then finding the BRCs assigned to those Resources in LaborTypes data
			List<string> resourceNames = refreshedModel.SkillMixRows.Where(s => !string.IsNullOrWhiteSpace(s.ResourceNew) && s.Included).Select(r => r.ResourceNew).Distinct().ToList();
			ICollection<string> historicalResourceNames = refreshedModel.SkillMixRows.Where(s => string.IsNullOrWhiteSpace(s.ResourceNew)).Select(r => r.ResourceOld).Distinct().ToList();
			resourceNames.AddRange(historicalResourceNames);

			foreach (string resourceName in resourceNames.Distinct())
			{
				ICollection<LaborTypeDataModelView> resourceLaborTypes = laborTypes.Where(l => l.ResourceName == resourceName).ToList();

				decimal totalHoursBRCs = resourceLaborTypes.SelectMany(x => x.Spreads).Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) >= Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);

				foreach (CommonDisclosureModelView refreshedRow in refreshedModel.CommonDisclosureRows.Where(r => r.ResourceID == resourceName).ToList())
				{
					ICollection<LaborTypeDataModelView> laborTypeDataModelViews = resourceLaborTypes.Where(l => l.BusinessResourceCodeName.NullEmptyEquals(refreshedRow.BusinessResourceID)).ToList();

					if (!string.IsNullOrEmpty(refreshedRow.BusinessResourceID) && !laborTypeDataModelViews.Any())
					{
						// No labor types found that match this Resource/BRC combo, this BRC is invalid so we need to remove the BRC
						refreshedModel.CommonDisclosureRows.Remove(refreshedRow);
						continue;
					}

					refreshedRow.ProposedHours = laborTypeDataModelViews.SelectMany(x => x.Spreads).Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) >= Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
					refreshedRow.GrandTotalHours = refreshedRow.ProposedHours; // RMS does not do UCOT
					// If this is not a Manual Skill Mix and the BRC is set, then the historical hours is a percentage of the real Historical Hours
					// Percentage is calculated based off the % calculated by BRC Sum / total BRCs Sum for this Resource
					if (!isManual && !string.IsNullOrWhiteSpace(refreshedRow.BusinessResourceID))
					{
						ICollection<string> legacyLinkedResourceIds = refreshedModel.SkillMixRows.Where(r => r.ResourceNew == resourceName && r.Included).Select(l => l.ResourceOld).ToList();
						decimal realHistoricalHours = resourceHours.Where(r => legacyLinkedResourceIds.Contains(r.ResourceName)).Sum(l => l.TotalHours);
						decimal brcHistoricalHours = 1m;
						if (totalHoursBRCs != 0)
						{
							brcHistoricalHours = refreshedRow.ProposedHours / totalHoursBRCs;
						}
						
						refreshedRow.HistoricalHours = realHistoricalHours * brcHistoricalHours;
					}

					if (currentCommonDisclosureData != null)
					{
						// Merge any old data into this row (needs to match both ResourceID and BRC ID), special checking if BRC ID is null/empty
						CommonDisclosureModelView disclosureRow = currentCommonDisclosureData.FirstOrDefault(r => r.ResourceID == refreshedRow.ResourceID &&
							r.BusinessResourceID.NullEmptyEquals(refreshedRow.BusinessResourceID));

						if (disclosureRow != null)
						{
							refreshedRow.BOEID = disclosureRow.BOEID;
							refreshedRow.Rationale = disclosureRow.Rationale;
							refreshedRow.Included = disclosureRow.Included;
							refreshedRow.CommonDisclosureSkillMixID = disclosureRow.CommonDisclosureSkillMixID;
							refreshedRow.IsUserInput = disclosureRow.IsUserInput;
							if (isManual)
							{
								refreshedRow.HistoricalHours = disclosureRow.HistoricalHours;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Copy Data from Matching Skill Mix Rows
		/// </summary>
		/// <param name="resourceHours">The Historical Resource Hours</param>
		/// <param name="laborTypes">The labor type data</param>
		/// <param name="currentSkillMixData">Current Skill Mix Data</param>
		/// <param name="refreshedModel">The Refreshed SKill Mix Model</param>
		/// <param name="isBRCEnabled">Is BRC Enabled for this workspace</param>
		/// <param name="isManual">Is this a manual SkillMix</param>
		private static void CopyMatchingSkillMixRowDataRMS(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
			RefreshSkillMixModelView refreshedModel, bool isBRCEnabled, bool isManual)
		{
			if (laborTypes == null)
			{
				throw new ArgumentNullException(nameof(laborTypes));
			}

			if (refreshedModel == null)
			{
				throw new ArgumentNullException(nameof(refreshedModel));
			}

			if (currentSkillMixData != null && currentSkillMixData.Any())
			{
				foreach (SkillMixModelView refreshedRow in refreshedModel.SkillMixRows.ToList())
				{
					// Find the matching current rows
					ICollection<SkillMixModelView> currentRows = currentSkillMixData.Where(r => r.ResourceOld.NullEmptyEquals(refreshedRow.ResourceOld)).ToList();
					if (currentRows.Any())
					{
						bool anyValidCurrentRows = false;

						foreach (SkillMixModelView currentRow in currentRows)
						{
							// make sure we have a Labor Types match for Resource
							ICollection<LaborTypeDataModelView> laborTypeDataModelViews = laborTypes.Where(l => !string.IsNullOrWhiteSpace(currentRow.ResourceNew) && l.ResourceName == currentRow.ResourceNew).ToList();

							// is this a user override row?
							if (string.IsNullOrWhiteSpace(currentRow.ResourceNew) && currentRow.IsUserInput && currentRow.Included)
							{
								// leave the row as-is, this row has no ResourceID (ResourceNew), but has been overwritten by the user
							}
							else
							{
								// Check for an invalid Resource selected, remove the Resource selected
								if (!laborTypeDataModelViews.Any())
								{
									currentRow.ResourceNew = string.Empty;
									currentRow.Included = false;
									currentRow.IsUserInput = false;
								}
							}

							// Merge the two rows
							currentRow.HistoricalHours = currentRow.HistoricalHours == 0m ? 0m : currentRow.HistoricalHours;
							currentRow.ResourceNew = currentRow.ResourceNew ?? string.Empty;

							anyValidCurrentRows = true;
							if (!isManual)
							{
								refreshedModel.SkillMixRows.Add(currentRow);
							}

							if (!string.IsNullOrWhiteSpace(currentRow.ResourceNew))
							{
								// user selected a Resource ID, so Included has to be true
								currentRow.Included = true;

								// Find the Proposed Hours for this Resource
								currentRow.ProposedHours = laborTypeDataModelViews.SelectMany(x => x.Spreads).Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) < Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
							}
							else
							{
								// No resource selected, zero out the proposed hours.  BOE Skill Mix % will be 0% auto-calculated later
								currentRow.ProposedHours = 0m;
							}

							if (!isManual)
							{
								decimal realHistoricalHours = resourceHours.Where(r => r.ResourceName == currentRow.ResourceOld).Sum(l => l.TotalHours);
								currentRow.HistoricalHours = realHistoricalHours;
							}

							if (isBRCEnabled)
							{
								// Create matching rows in Common Disclosures for ResourceNew
								ICollection<string> brcNames = laborTypeDataModelViews.Where(r => !string.IsNullOrWhiteSpace(r.BusinessResourceCodeName)).Select(l => l.BusinessResourceCodeName).Distinct().ToList();
								if (string.IsNullOrWhiteSpace(currentRow.ResourceNew))
								{
									// create a Not included row
									refreshedModel.CommonDisclosureRows.Add(
											new CommonDisclosureModelView
											{
												ResourceID = currentRow.ResourceOld, // Set the ID to the Historical Resource ID
												BusinessResourceID = string.Empty,
												Included = false,
												HistoricalHours = currentRow.HistoricalHours
											}
										);
								}
								else if (brcNames.Any())
								{
									foreach (string brcName in brcNames)
									{
										if (!refreshedModel.CommonDisclosureRows.Any(c => c.ResourceID == currentRow.ResourceNew && c.BusinessResourceID == brcName))
										{
											refreshedModel.CommonDisclosureRows.Add(
												new CommonDisclosureModelView
												{
													ResourceID = currentRow.ResourceNew,
													BusinessResourceID = brcName,
													Included = true
												}
											);
										}
									}
								}
							}
						}

						if (anyValidCurrentRows && !isManual)
						{
							// remove the refreshed row to make way for the currentRows
							refreshedModel.SkillMixRows.Remove(refreshedRow);
						}
					}
					else if (isBRCEnabled)
					{
						// still may need to add to common disclosure
						// create a Not included row
						refreshedModel.CommonDisclosureRows.Add(
								new CommonDisclosureModelView
								{
									ResourceID = refreshedRow.ResourceOld, // Set the ID to the Historical Resource ID
									BusinessResourceID = string.Empty,
									Included = false,
									HistoricalHours = refreshedRow.HistoricalHours
								}
							);
					}
				}
			}
		}

		/// <summary>
		/// Mock out the Resource Hours from the Current Skill Mix Data
		/// </summary>
		/// <param name="currentSkillMixData">Current Skill Mix Data</param>
		/// <returns>Mocked out resource hours</returns>
		private static ICollection<MOQTypeSelectionTableDataResourceHoursDTO> MockResourceHours(ICollection<SkillMixModelView> currentSkillMixData)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>(currentSkillMixData.Count);
			foreach (SkillMixModelView skillMix in currentSkillMixData)
			{
				if (!string.IsNullOrWhiteSpace(skillMix.ResourceOld))
				{
					hours.Add(new MOQTypeSelectionTableDataResourceHoursDTO
					{
						BOEID = skillMix.BOEID,
						BOETaskElementID = skillMix.BOETaskElementID,
						ResourceName = skillMix.ResourceOld,
						TotalHours = skillMix.HistoricalHours,
						WbsHours = skillMix.HistoricalHours
					});
				}
			}

			return hours;
		}

		/// <summary>
		/// Filter out bad data inside the current lists
		/// </summary>
		/// <param name="currentSkillMixData">The current skill mix data</param>
		/// <param name="currentCommonDisclosureData">Current Common Disclosure data</param>
		/// <param name="resourceHours">The Resource Hours</param>
		/// <param name="isManual">If the Historical Resource/Hours are Manually input or not</param>
		private static void FilterBadDataRMS(ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
			ICollection<CommonDisclosureModelView> currentCommonDisclosureData, ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, bool isManual)
		{
			if (laborTypes.Any())
			{
				HashSet<string> resources = laborTypes.Select(l => l.ResourceName).Distinct().ToHashSet();
				HashSet<string> brcs = laborTypes.Select(l => l.BusinessResourceCodeName).Distinct().ToHashSet();
				HashSet<string> historicalResources = resourceHours.Select(r => r.ResourceName).Distinct().ToHashSet();

				foreach (SkillMixModelView skillMixModel in currentSkillMixData)
				{
					if (!string.IsNullOrWhiteSpace(skillMixModel.ResourceNew) && !resources.Contains(skillMixModel.ResourceNew) && !historicalResources.Contains(skillMixModel.ResourceOld))
					{
						// this skill mix model is pointing towards a missing Resource, remove the resource name
						skillMixModel.ResourceNew = string.Empty;
						skillMixModel.ProposedHours = 0m;
						skillMixModel.BOESkillMix = 0m;
						skillMixModel.Included = false;
					}
				}

				foreach (CommonDisclosureModelView commonDisclosureModel in currentCommonDisclosureData.ToList())
				{
					// Remove bad resources
					if (!string.IsNullOrWhiteSpace(commonDisclosureModel.ResourceID) && !resources.Contains(commonDisclosureModel.ResourceID) && !historicalResources.Contains(commonDisclosureModel.ResourceID))
					{
						commonDisclosureModel.ResourceID = string.Empty;
						commonDisclosureModel.HistoricalHours = 0m;
						commonDisclosureModel.LaborSkillMix = 0m;
					}

					if (!string.IsNullOrWhiteSpace(commonDisclosureModel.BusinessResourceID) && !brcs.Contains(commonDisclosureModel.BusinessResourceID))
					{
						// this RMS skill mix model is pointing towards a missing Resource, remove the resource name
						commonDisclosureModel.BusinessResourceID = string.Empty;
						commonDisclosureModel.ProposedHours = 0m;
						commonDisclosureModel.BOESkillMix = 0m;
						commonDisclosureModel.Included = false;
					}

					// if the row does not have resource or BRC set, then blank it out.
					if (string.IsNullOrEmpty(commonDisclosureModel.ResourceID) && string.IsNullOrEmpty(commonDisclosureModel.BusinessResourceID))
					{
						commonDisclosureModel.HistoricalHours = 0m;
						commonDisclosureModel.LaborSkillMix = 0m;
						commonDisclosureModel.ProposedHours = 0m;
						commonDisclosureModel.BOESkillMix = 0m;
						commonDisclosureModel.Included = false;
					}
				}
			}
			else if (isManual)
			{
				// these skill mix model are pointing towards missing Resources, remove the resource name
				foreach (SkillMixModelView skillMixModel in currentSkillMixData)
				{
					skillMixModel.ResourceNew = string.Empty;
					skillMixModel.ProposedHours = 0m;
					skillMixModel.BOESkillMix = 0m;
					skillMixModel.Included = false;
				}

				// Clear out the common disclosure data
				currentCommonDisclosureData.Clear();
			}
			else
			{
				currentSkillMixData.Clear();
				currentCommonDisclosureData.Clear();
			}
		}

		/// <summary>
		/// Calculates the BOESkillMix Percentage
		/// </summary>
		/// <param name="refreshedModel"></param>
		private static void CalculateBoeSkillMixPercentage(RefreshSkillMixModelView refreshedModel)
		{
			// Set BOE Skill Mix Percent on Skill Mix table
			foreach (SkillMixModelView row in refreshedModel.SkillMixRows)
			{
				row.LaborSkillMix = refreshedModel.SkillMixTotals.HistoricalHours == 0m ? 0m : row.HistoricalHours * 100.0m / refreshedModel.SkillMixTotals.HistoricalHours;
				if (row.Included && refreshedModel.SkillMixTotals.ProposedLegacyResource != 0.0m)
				{
					row.BOESkillMix = row.ProposedHours * 100.0m / refreshedModel.SkillMixTotals.ProposedLegacyResource;
				}
				else
				{
					row.BOESkillMix = 0.0m;
				}
			}

			// Set BOE Skill Mix Percent on Common Disclosure table
			foreach (CommonDisclosureModelView row in refreshedModel.CommonDisclosureRows)
			{
				row.LaborSkillMix = refreshedModel.CommonDisclosureTotals.HistoricalHours == 0m ? 0m : row.HistoricalHours * 100.0m / refreshedModel.CommonDisclosureTotals.HistoricalHours;
				if (row.Included && refreshedModel.CommonDisclosureTotals.ProposedLegacyResource != 0.0m)
				{
					row.BOESkillMix = row.ProposedHours * 100.0m / refreshedModel.CommonDisclosureTotals.ProposedLegacyResource;
				}
				else
				{
					row.BOESkillMix = 0.0m;
				}
			}

			// Set BOE Skill Mix Percent on Skill Mix Summary table
			foreach (SkillMixSummaryModelView row in refreshedModel.SkillMixSummaryRows)
			{
				row.HistoricalSkillMix = refreshedModel.SkillMixSummaryTotals.HistoricalHours == 0m ? 0m : row.HistoricalHours * 100.0m / refreshedModel.SkillMixSummaryTotals.HistoricalHours;
				if (refreshedModel.SkillMixSummaryTotals.ProposedLegacyResource != 0.0m)
				{
					row.ProposedSkillMix = row.ProposedLegacyResource * 100.0m / refreshedModel.SkillMixSummaryTotals.ProposedLegacyResource;
				}
				else
				{
					row.ProposedSkillMix = 0.0m;
				}
			}

			// BoeSkillMix Totals
			refreshedModel.SkillMixTotals.ProposedSkillMix = refreshedModel.SkillMixRows.Where(d => d.Included).Sum(s => s.BOESkillMix ?? 0.0m);
			refreshedModel.CommonDisclosureTotals.ProposedSkillMix = refreshedModel.CommonDisclosureRows.Where(d => d.Included).Sum(s => s.BOESkillMix ?? 0.0m);
			refreshedModel.SkillMixSummaryTotals.ProposedSkillMix = refreshedModel.SkillMixSummaryRows.Sum(s => s.ProposedSkillMix ?? 0.0m);
		}

		/// <summary>
		/// Apply the appropriate canned‑response rationale for each skill‑mix row.
		/// </summary>
		/// <param name="refreshedModel">Skill‑mix data (contains the rows to evaluate).</param>
		private static void ApplyRationaleCannedResponses(RefreshSkillMixModelView refreshedModel)
		{
			foreach (SkillMixSummaryModelView row in refreshedModel.SkillMixSummaryRows)
			{

				// Checks if the proposed skill mix and historical are equal.
				bool proposedSkillMixAndHistoricalEqual = row.ProposedSkillMix.HasValue && row.ProposedSkillMix.Value == row.HistoricalSkillMix;

				// Difference expressed as an absolute percentage.
				decimal percentDifference = Math.Abs(row.ProposedSkillMix.GetValueOrDefault() - row.HistoricalSkillMix);

				// Get the range of dates for Resource level.
				//bool hasHoursThrough2028Only = HasProposedHoursInRange(row, 2020, 2028) && !HasProposedHoursInRange(row, 2029, int.MaxValue);
				//bool hasHoursThrough2028AndPast2029 = HasProposedHoursInRange(row, 2020, 2028) && HasProposedHoursInRange(row, 2029, int.MaxValue);
				//bool hasHoursPast2029 = !HasProposedHoursInRange(row, 2020, 2028) && HasProposedHoursInRange(row, 2029, int.MaxValue);

				bool hasHoursThrough2028Only = true;
				bool hasHoursThrough2028AndPast2029 = true;
				bool hasHoursPast2029 = true;

				string cannedRationale = null;

				// Historical = Proposed.
				if (proposedSkillMixAndHistoricalEqual)
				{
					if (hasHoursThrough2028Only)
					{
						cannedRationale = Constants.SPACE_SKILL_MIX_SUMMARY_RATIONALE_EQUAL_TO_2028;
					}
					else if (hasHoursThrough2028AndPast2029)
					{
						cannedRationale = Constants.SPACE_SKILL_MIX_SUMMARY_RATIONALE_EQUAL_2028_2029;
					}
					else if (hasHoursPast2029)
					{
						cannedRationale = Constants.SPACE_SKILL_MIX_SUMMARY_RATIONALE_EQUAL_2029;
					}
				}
				// Historical <= 5% difference from Skill Mix
				else if (percentDifference <= 5m)
				{
					if (hasHoursThrough2028Only)
					{
						cannedRationale = Constants.SPACE_SKILL_MIX_SUMMARY_RATIONALE_LESS_THAN_5_PERCENT_TO_2028;
					}
					else if (hasHoursThrough2028AndPast2029)
					{
						cannedRationale = Constants.SPACE_SKILL_SUMMARY_RATIONALE_LESS_THAN_5_PERCENT_2028_2029;
					}
					else if (hasHoursPast2029)
					{
						cannedRationale = Constants.SPACE_SKILL_SUMMARY_RATIONALE_LESS_THAN_5_PERCENT_2029;
					}
				}
				// Historical > 5% difference from Skill Mix
				else if (percentDifference > 5m)
				{
					if (hasHoursThrough2028Only)
					{
						//row.RationalePlaceholderText = Constants.SPACE_SKILL_SUMMARY_RATIONALE_PLACEHOLDER_INCLUDE_GREATER_THAN_5_PERCENT;
						row.UsesMixedCannedResponseAndUserInput = true;
						row.RationalePlaceholderText = Constants.SPACE_SKILL_SUMMARY_RATIONALE_PLACEHOLDER_GREATER_THAN_5_PERCENT;
						row.RationaleMixedCannedResponse = Constants.SPACE_SKILL_SUMMARY_RATIONALE_GREATER_THAN_5_PERCENT_2028_2029;
					}
					else if (hasHoursThrough2028AndPast2029)
					{
						row.UsesMixedCannedResponseAndUserInput = true;
						row.RationalePlaceholderText = Constants.SPACE_SKILL_SUMMARY_RATIONALE_PLACEHOLDER_GREATER_THAN_5_PERCENT;
						row.RationaleMixedCannedResponse = Constants.SPACE_SKILL_SUMMARY_RATIONALE_GREATER_THAN_5_PERCENT_2028_2029;
					}
					else if (hasHoursPast2029)
					{
						row.UsesMixedCannedResponseAndUserInput = true;
						row.RationalePlaceholderText = Constants.SPACE_SKILL_SUMMARY_RATIONALE_PLACEHOLDER_GREATER_THAN_5_PERCENT;
						row.RationaleMixedCannedResponse = Constants.SPACE_SKILL_SUMMARY_RATIONALE_GREATER_THAN_5_PERCENT_2029;
					}
				}

				// If canned rationale is provided then set it, otherwise keep it the same.
				if (cannedRationale != null && !row.UsesMixedCannedResponseAndUserInput)
				{
					row.Rationale = cannedRationale;
					row.IsRationaleReadOnly = true;
				}
				else
				{
					row.IsRationaleReadOnly = false;
				}
			}
		}

		///// <summary>
		///// Returns true if the row has **any** proposed hours > 0 in the inclusive
		///// year range <paramref name="startYear"/> … <paramref name="endYear"/>.
		///// </summary>
		//private static bool HasProposedHoursInRange(SkillMixSummaryModelView row, int startYear, int endYear)
		//{
		//	// Assume a dictionary; replace with your actual storage mechanism.
		//	if (row.ProposedHoursByYear == null)
		//	{
		//		return false;
		//	}

		//	foreach (var kvp in row.ProposedHoursByYear)
		//	{
		//		int year = kvp.Key;
		//		decimal hrs = kvp.Value;

		//		if (year >= startYear && year <= endYear && hrs > 0m)
		//		{
		//			return true;
		//		}

		//	}

		//	return false;
		//}

		/// <summary>
		/// Calculates the SkillMix/CD row Totals
		/// </summary>
		/// <param name="refreshedModel">The skill mix model view to calculate on</param>
		private static void CalculateSkillMixTotals(RefreshSkillMixModelView refreshedModel)
		{
			// Skill Mix Totals
			refreshedModel.SkillMixTotals.HistoricalHours = refreshedModel.SkillMixRows.Sum(s => s.HistoricalHours);
			refreshedModel.SkillMixTotals.HistoricalSkillMix = 100.0m;
			refreshedModel.SkillMixTotals.ProposedLegacyResource = refreshedModel.SkillMixRows.Where(d => d.Included).Sum(s => s.ProposedHours);

			// Common Disclosure Totals
			refreshedModel.CommonDisclosureTotals.HistoricalHours = refreshedModel.CommonDisclosureRows.Sum(s => s.HistoricalHours);
			refreshedModel.CommonDisclosureTotals.HistoricalSkillMix = 100.0m;
			refreshedModel.CommonDisclosureTotals.ProposedLegacyResource = refreshedModel.CommonDisclosureRows.Where(d => d.Included).Sum(s => s.ProposedHours);
			refreshedModel.CommonDisclosureTotals.UCOTHours = refreshedModel.CommonDisclosureRows.Sum(s => s.UCOTHours);

			// Skill Mix Summary Totals
			refreshedModel.SkillMixSummaryTotals.HistoricalHours = refreshedModel.SkillMixSummaryRows.Sum(s => s.HistoricalHours);
			refreshedModel.SkillMixSummaryTotals.HistoricalSkillMix = 100.0m;
			refreshedModel.SkillMixSummaryTotals.ProposedLegacyResource = refreshedModel.SkillMixSummaryRows.Sum(s => s.ProposedLegacyResource);
			refreshedModel.SkillMixSummaryTotals.ProposedBrc = refreshedModel.SkillMixSummaryRows.Sum(s => s.ProposedBrc);
			refreshedModel.SkillMixSummaryTotals.UCOTHours = refreshedModel.SkillMixSummaryRows.Sum(s => s.UCOTHours);
		}

		/// <summary>
		/// Remove SkillMix and CommonDisclosure rows as needed
		/// </summary>
		/// <param name="refreshedModel">The skill mix model view to cleanup</param>
		private static void CleanupData(RefreshSkillMixModelView refreshedModel)
		{
			if (refreshedModel == null)
			{
				throw new ArgumentNullException(nameof(refreshedModel));
			}

			// Check for duplicate Proposed Resources
			HashSet<string> distinctProposedResources = new HashSet<string>();
			foreach (SkillMixModelView row in refreshedModel.SkillMixRows.Where(x => !string.IsNullOrEmpty(x.ResourceNew)))
			{
				if (!distinctProposedResources.Add(row.ResourceNew))
				{
					// Duplicate found - Set proposed hours to 0
					row.ProposedHours = 0;
				}
			}
		}
	}
}
