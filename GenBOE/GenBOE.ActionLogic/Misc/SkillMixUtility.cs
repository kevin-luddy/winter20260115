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
		/// <param name="isBRCEnabled">Is BRC Enabled for CD row check.</param>
		/// <param name="isManual">If the Historical Resource/Hours are Manually input or not</param>
		/// <returns>Refreshed/Recalculated Skill Mix Model View</returns>
		public static RefreshSkillMixModelView RefreshSkillMixTables(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
			ICollection<CommonDisclosureModelView> currentCommonDisclosureData, bool isBRCEnabled, bool isManual, DateTime? taskEndDate)
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

			// Only do Common Disclosure if the task is past the 1LMX start date
			isBRCEnabled = isBRCEnabled && taskEndDate.HasValue && taskEndDate >= Utilities.OneLmxStartDate;

			RefreshSkillMixModelView refreshedModel = new RefreshSkillMixModelView();
			laborTypes = laborTypes == null ? new List<LaborTypeDataModelView>() : laborTypes.Where(l => l.RateType == RateType.Hours).ToList();

			if (isManual)
			{
				// Mock out the Resource Hours from input data
				resourceHours = MockResourceHours(currentSkillMixData);
			}

			bool isSpace = SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.SpaceSystems;

			if (isSpace || resourceHours.Any())
			{
				bool addBlankRow = !isSpace;

				// filter out bad data in currentSkillMixData
				FilterBadData(laborTypes, currentSkillMixData, currentCommonDisclosureData, resourceHours, isManual, isSpace);

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
					// This can be the case if the MOQ Type was not Historical/Comparitive but user still wanted to use that MOQ Type as a reference
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

				if (isSpace)
				{
					CopyMatchingSkillMixRowDataSpace(resourceHours, laborTypes, currentSkillMixData, refreshedModel, isManual);
				}
				else
				{
					CopyMatchingSkillMixRowDataRMS(resourceHours, laborTypes, currentSkillMixData, refreshedModel, isBRCEnabled, isManual);
					CleanupNewSkillMixRow(currentSkillMixData);
				}

				if (isBRCEnabled)
				{
					if (isSpace)
					{
						CreateCommonDisclosureRowsSpace(resourceHours, laborTypes, currentCommonDisclosureData, 
							refreshedModel);
					}
					else
					{
						CreateCommonDisclosureRowsRMS(resourceHours, laborTypes, currentCommonDisclosureData, 
							refreshedModel, isManual);
						CleanUpCommonDisclosureRows(currentSkillMixData, refreshedModel);

					}
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

			CleanupData(refreshedModel, isSpace);
			CalculateSkillMixTotals(refreshedModel);
			CalculateBoeSkillMixPercentage(refreshedModel);

			// reorder the lists
			refreshedModel.SkillMixRows = refreshedModel.SkillMixRows.OrderBy(r => string.IsNullOrWhiteSpace(r.ResourceOld)).ThenBy(r => r.ResourceOld).ToList();
			refreshedModel.CommonDisclosureRows = refreshedModel.CommonDisclosureRows.OrderBy(r => string.IsNullOrWhiteSpace(r.ResourceID)).ThenBy(r => r.ResourceID).ThenBy(s => s.BusinessResourceID).ToList();

			return refreshedModel;
		}

		/// <summary>
		/// Set historical hours to zero for added rows
		/// </summary>
		/// <param name="currentSkillMixData">Collection of skillmix data</param>
		private static void CleanupNewSkillMixRow(ICollection<SkillMixModelView> currentSkillMixData)
		{
			List<string> resourceOldList = new List<string>();
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
			ICollection<string> newLinkedResourceIds = refreshedModel.SkillMixRows.Where(r => !string.IsNullOrWhiteSpace(r.ResourceNew)).Select(l => l.ResourceNew).Distinct().ToList();
			foreach (CommonDisclosureModelView item in refreshedModel.CommonDisclosureRows)
			{
				if (newLinkedResourceIds.Contains(item.ResourceID))
				{
					item.HistoricalHours = currentSkillMixData.Where(sm => item.ResourceID == sm.ResourceNew).Sum(l => l.HistoricalHours);
				}
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

								if (!isManual)
								{
									decimal realHistoricalHours = resourceHours.Where(r => r.ResourceName == currentRow.ResourceOld).Sum(l => l.TotalHours);
									currentRow.HistoricalHours = realHistoricalHours;
								}
							}
							else
							{
								// No resource selected, zero out the proposed hours.  BOE Skill Mix % will be 0% auto-calculated later
								currentRow.ProposedHours = 0m;
							}

							if (isBRCEnabled)
							{
								// Create matching rows in Common Disclosures for ResourceNew
								ICollection<string> brcNames = laborTypeDataModelViews.Where(r => !string.IsNullOrWhiteSpace(r.BusinessResourceCodeName)).Select(l => l.BusinessResourceCodeName).Distinct().ToList();
								if (brcNames.Any())
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
								else if (string.IsNullOrWhiteSpace(currentRow.ResourceNew))
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
							}
						}

						if (anyValidCurrentRows && !isManual)
						{
							// remove the refreshed row to make way for the currentRows
							refreshedModel.SkillMixRows.Remove(refreshedRow);
						}
					}
				}
			}
		}

		/// <summary>
		/// Copy Data from Matching Skill Mix Rows
		/// </summary>
		/// <param name="resourceHours">Historical Resource Hours</param>
		/// <param name="laborTypes">The labor type data</param>
		/// <param name="currentSkillMixData">Current Skill Mix Data</param>
		/// <param name="refreshedModel">The Refreshed SKill Mix Model</param>
		/// <param name="isManual">Is this a manual SkillMix</param>
		private static void CopyMatchingSkillMixRowDataSpace(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
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

			foreach (SkillMixModelView row in refreshedModel.SkillMixRows)
			{
				// Set the Resource fields to be equal
				row.ResourceNew = row.ResourceOld;
				// Empty out the proposed hours until a match is made
				row.ProposedHours = 0m;
			}

			foreach (LaborTypeDataModelView labor in laborTypes)
			{
				if (!string.IsNullOrWhiteSpace(labor.ResourceName))
				{
					// Find the Proposed Hours for this Resource
					decimal proposedHours = labor.Spreads.Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) < Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
					SkillMixModelView historicalSkillMix = refreshedModel.SkillMixRows.FirstOrDefault(s => s.ResourceOld == labor.ResourceName);
					if (historicalSkillMix != null)
					{
						// If match, add the labor data (summation to group all proposed hours for this matching Resource)
						historicalSkillMix.ProposedHours += proposedHours;
						historicalSkillMix.Included = true;

						if (!isManual && historicalSkillMix.ProposedHours != 0m)
						{
							decimal realHistoricalHours = resourceHours.Where(r => r.ResourceName == historicalSkillMix.ResourceOld).Sum(l => l.TotalHours);
							historicalSkillMix.HistoricalHours = realHistoricalHours;
						}
					}
					else
					{
						// create new row
						SkillMixModelView newRow = new SkillMixModelView
						{
							ResourceNew = labor.ResourceName,
							ResourceOld = labor.ResourceName,
							HistoricalHours = 0m, // No historical linkage
							ProposedHours = proposedHours,
							Included = true
						};

						refreshedModel.SkillMixRows.Add(newRow);
					}
				}
			}

			// Add back rationale from the Current data
			foreach (SkillMixModelView row in refreshedModel.SkillMixRows)
			{
				SkillMixModelView currentData = currentSkillMixData.FirstOrDefault(s => s.ResourceNew == row.ResourceNew);
				if (currentData != null)
				{
					row.Rationale = currentData.Rationale;
				}
			}
		}

		/// <summary>
		/// Create the Common Disclosure Rows from the data
		/// </summary>
		/// <param name="resourceHours">The resource hours</param>
		/// <param name="laborTypes">labor type data</param>
		/// <param name="currentCommonDisclosureData">Current Common Disclosure Data</param>
		/// <param name="refreshedModel">The Refreshed Skill Mix Model</param>
		private static void CreateCommonDisclosureRowsSpace(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours,
			ICollection<LaborTypeDataModelView> laborTypes, ICollection<CommonDisclosureModelView> currentCommonDisclosureData,
			RefreshSkillMixModelView refreshedModel)
		{
			if (laborTypes == null)
			{
				throw new ArgumentNullException(nameof(laborTypes));
			}

			if (refreshedModel == null)
			{
				throw new ArgumentNullException(nameof(refreshedModel));
			}

			// First add the Historical Hours
			refreshedModel.CommonDisclosureRows.Clear();
			ICollection<IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO>> groupedResourceHours = resourceHours.GroupBy(r => r.ResourceName).OrderBy(t => t.Key).ToList();
			foreach (IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO> grouping in groupedResourceHours)
			{
				// Inner grouping by BRC for the Resource
				ICollection<IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO>> brcGroupings = grouping.GroupBy(r => r.BRCName).OrderBy(t => t.Key).ToList();
				foreach (IGrouping<string, MOQTypeSelectionTableDataResourceHoursDTO> brcGrouping in brcGroupings)
				{
					refreshedModel.CommonDisclosureRows.Add(
						new CommonDisclosureModelView
						{
							HistoricalHours = brcGrouping.Sum(b => b.TotalHours),
							ResourceID = grouping.Key,
							BusinessResourceID = brcGrouping.Key,
							Included = false,
							ProposedHours = 0m,
							UCOTHours = 0m,
							GrandTotalHours = 0m
						}
					);
				}
			}

			// Merge in the Labor Types
			foreach (LaborTypeDataModelView labor in laborTypes.ToList())
			{
				if (!string.IsNullOrWhiteSpace(labor.BusinessResourceCodeName))
				{
					// Find the Proposed Hours for this Resource
					decimal proposedHours = labor.Spreads.Where(s => DateTime.Parse(s.LaborSpreadDate).Normalize(DateTimePrecision.Month) >= Utilities.OneLmxStartDate).Sum(sp => sp.LaborSpreadValue.HasValue ? sp.LaborSpreadValue.Value : 0.0m);
					CommonDisclosureModelView historicalSkillMix = refreshedModel.CommonDisclosureRows.FirstOrDefault(s => s.ResourceID == labor.ResourceName && s.BusinessResourceID == labor.BusinessResourceCodeName);
					if (historicalSkillMix != null)
					{
						// If match, add the labor data
						historicalSkillMix.ProposedHours += proposedHours;
						historicalSkillMix.UCOTHours = labor.UcotSpreads?.Sum(x => x.LaborSpreadValue ?? 0m) ?? 0m; 
						historicalSkillMix.GrandTotalHours = historicalSkillMix.ProposedHours + historicalSkillMix.UCOTHours;
						historicalSkillMix.Included = true;
					}
					else
					{
						// create new row
						CommonDisclosureModelView newRow = new CommonDisclosureModelView
						{
							ResourceID = labor.ResourceName,
							BusinessResourceID = labor.BusinessResourceCodeName,
							HistoricalHours = 0m,
							ProposedHours = proposedHours,
							Included = true,
							UCOTHours = labor.UcotSpreads?.Sum(x => x.LaborSpreadValue ?? 0m) ?? 0m
						};

						newRow.GrandTotalHours = newRow.ProposedHours + newRow.UCOTHours;

						refreshedModel.CommonDisclosureRows.Add(newRow);
					}
				}
			}

			// Match the current rows to get the rationale (only thing editable)
			foreach (CommonDisclosureModelView modelView in refreshedModel.CommonDisclosureRows)
			{
				CommonDisclosureModelView match = currentCommonDisclosureData.FirstOrDefault(d => d.ResourceID == modelView.ResourceID && d.BusinessResourceID == modelView.BusinessResourceID);
				if (match != null)
				{
					modelView.Rationale = match.Rationale;
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
		/// <param name="isSpace">Whether this is Space or not</param>
		private static void FilterBadData(ICollection<LaborTypeDataModelView> laborTypes, ICollection<SkillMixModelView> currentSkillMixData,
			ICollection<CommonDisclosureModelView> currentCommonDisclosureData, ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, bool isManual, bool isSpace)
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

					if (!isSpace && !string.IsNullOrWhiteSpace(commonDisclosureModel.BusinessResourceID) && !brcs.Contains(commonDisclosureModel.BusinessResourceID))
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
				if (row.Included && refreshedModel.SkillMixTotals.ProposedHours != 0.0m)
				{
					row.BOESkillMix = row.ProposedHours * 100.0m / refreshedModel.SkillMixTotals.ProposedHours;
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
				if (row.Included && refreshedModel.CommonDisclosureTotals.ProposedHours != 0.0m)
				{
					row.BOESkillMix = row.ProposedHours * 100.0m / refreshedModel.CommonDisclosureTotals.ProposedHours;
				}
				else
				{
					row.BOESkillMix = 0.0m;
				}
			}

			// BoeSkillMix Totals
			refreshedModel.SkillMixTotals.BoeSkillMix = refreshedModel.SkillMixRows.Where(d => d.Included).Sum(s => s.BOESkillMix ?? 0.0m);
			refreshedModel.CommonDisclosureTotals.BoeSkillMix = refreshedModel.CommonDisclosureRows.Where(d => d.Included).Sum(s => s.BOESkillMix ?? 0.0m);
		}

		/// <summary>
		/// Calculates the SkillMix/CD row Totals
		/// </summary>
		/// <param name="refreshedModel">The skill mix model view to calculate on</param>
		private static void CalculateSkillMixTotals(RefreshSkillMixModelView refreshedModel)
		{
			// Skill Mix Totals
			refreshedModel.SkillMixTotals.HistoricalHours = refreshedModel.SkillMixRows.Sum(s => s.HistoricalHours);
			refreshedModel.SkillMixTotals.LaborSkillMix = 100.0m;
			refreshedModel.SkillMixTotals.ProposedHours = refreshedModel.SkillMixRows.Where(d => d.Included).Sum(s => s.ProposedHours);

			// Common Disclosure Totals
			refreshedModel.CommonDisclosureTotals.HistoricalHours = refreshedModel.CommonDisclosureRows.Sum(s => s.HistoricalHours);
			refreshedModel.CommonDisclosureTotals.LaborSkillMix = 100.0m;
			refreshedModel.CommonDisclosureTotals.ProposedHours = refreshedModel.CommonDisclosureRows.Where(d => d.Included).Sum(s => s.ProposedHours);
			refreshedModel.CommonDisclosureTotals.UCOTHours = refreshedModel.CommonDisclosureRows.Sum(s => s.UCOTHours);
			refreshedModel.CommonDisclosureTotals.GrandTotalHours = refreshedModel.CommonDisclosureTotals.ProposedHours + refreshedModel.CommonDisclosureTotals.UCOTHours;
		}

		/// <summary>
		/// Remove SkillMix and CommonDisclosure rows as needed
		/// </summary>
		/// <param name="refreshedModel">The skill mix model view to cleanup</param>
		/// <param name="isSpace">Is Space the current company mode?</param>
		private static void CleanupData(RefreshSkillMixModelView refreshedModel, bool isSpace)
		{
			if (refreshedModel == null)
			{
				throw new ArgumentNullException(nameof(refreshedModel));
			}

			if (isSpace)
			{
				// Remove Skill Mix rows where historical and proposed hours are zero
				List<SkillMixModelView> skillMixRowsToRemove = refreshedModel.SkillMixRows
					.Where(row => row.HistoricalHours == 0 && row.ProposedHours == 0)
					.ToList();

				foreach (SkillMixModelView item in skillMixRowsToRemove)
				{
					refreshedModel.SkillMixRows.Remove(item);
				}

				// Remove Common Disclosure rows where historical and proposed hours are zero
				List<CommonDisclosureModelView> commonDisclosureRowsToRemove = refreshedModel.CommonDisclosureRows
					.Where(row => row.HistoricalHours == 0 && row.ProposedHours == 0)
					.ToList();

				foreach (CommonDisclosureModelView item in commonDisclosureRowsToRemove)
				{
					refreshedModel.CommonDisclosureRows.Remove(item);
				}
			}
			else
			{
				// Check for duplicate Current Resources
				HashSet<string> distinctCurrentResources = new HashSet<string>();
				foreach (SkillMixModelView row in refreshedModel.SkillMixRows.Where(x => !string.IsNullOrEmpty(x.ResourceNew)))
				{
					if (!distinctCurrentResources.Add(row.ResourceNew))
					{
						// Duplicate found - Set proposed hours to 0
						row.ProposedHours = 0;
					}
				}
			}
		}
	}
}
