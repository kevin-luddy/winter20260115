// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
    using Microsoft.Practices.ObjectBuilder2;

    /// <summary>
    /// Trace Table Exporter
    /// </summary>
    public class TraceTableExporter : ITraceTableExporter
	{
		/// <summary>
		/// ctor
		/// </summary>
		public TraceTableExporter()
		{
		}

		/// <summary>
		/// Export Trace Table Data
		/// </summary>
		/// <param name="workspace">Full Workspace</param>
		/// <param name="settingsData">Trace Table Settings Data</param>
		/// <returns>Trace Table Data</returns>
		public ICollection<TraceTableBoeData> ExportTraceTableData(FullWorkspace workspace, TraceTableSettingsData settingsData)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			TraceTableBoeData boeData = new TraceTableBoeData();
			TraceTableBoeDataGroup boeDataGroup = new TraceTableBoeDataGroup();

			if (settingsData != null)
			{
                settingsData.GroupingField = "CalendarYear";
            }
			boeDataGroup.ChildData = ExportTraceTableDataGroup(workspace, settingsData);

			// Convert new TraceTableBoeDataGroup model to old TraceTableBoeData model
			boeDataGroup.ChildData.ForEach(x => boeData.ChildData.Add(new TraceTableBoeData(x)));

			// return child data - top level is empty and child data will contain the first summary field
			return boeData.ChildData;
		}

        /// <summary>
        /// Export Trace Table Data group
        /// </summary>
        /// <param name="workspace">Full Workspace</param>
        /// <param name="settingsData">Trace Table Settings Data</param>
        /// <returns>Trace Table Data group</returns>
        public ICollection<TraceTableBoeDataGroup> ExportTraceTableDataGroup(FullWorkspace workspace, TraceTableSettingsData settingsData)
        {
            _ = workspace ?? throw new ArgumentNullException(nameof(workspace));

            TraceTableBoeDataGroup boeData = new TraceTableBoeDataGroup();

            // Get labors, filter by Rate Type from settings data
            List<ResourceTypeDto> taskElementLabors = workspace.Boes
                .SelectMany(x => x.TaskElements)
                .SelectMany(x => x.taskElementLabors)
                .Where(x => (int)x.SpreadType == settingsData.RateType).ToList();

            // Filter by element of cost from the settings data
            ICollection<int> laborsToRemove = new Collection<int>();
            foreach (ResourceTypeDto labor in taskElementLabors)
            {
                ResourceDTO resource = workspace.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == labor.ResourceID);
                if (resource == null || !settingsData.ElementsOfCost.Contains((int)resource.ElementOfCost))
                {
                    laborsToRemove.Add(labor.Id);
                }
            }

            taskElementLabors.RemoveAll(x => laborsToRemove.Contains(x.Id));

            // Populate CLIN and WBS IDs for non-multi-clin-wbs
            foreach (ResourceTypeDto labor in taskElementLabors)
            {
                FullBoe boe = workspace.Boes.FirstOrDefault(x => x.Id == labor.BoeID);
                if (boe != null && !boe.IsMultiClinWbs)
                {
                    labor.CLINID = boe.CLINID;
                    labor.WBSID = boe.WBSID;
                }
            }

            ProcessLaborData(workspace, settingsData.SummaryFields.FirstOrDefault(), settingsData.SummaryFields.Skip(1).ToList(),
                taskElementLabors, boeData, settingsData.ShowYears, settingsData.GroupingField, settingsData.CustomGroupingField);

            // return child data - top level is empty and child data will contain the first summary field
            return boeData.ChildData;
        }

        /// <summary>
        /// Recursively process Labor Data for use with the Trace Table
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <param name="currentLevel">The current level of summary field</param>
        /// <param name="additionalLevels">Additional summary field levels</param>
        /// <param name="resourceTypes">resource types for the workspace</param>
        /// <param name="parent">parent level Trace Table BOE Data</param>
        /// <param name="includeYearlyData">If yearly data should be included</param>
		/// <param name="groupingField">Field to group trace table columns by</param>
		/// <param name="customGroupingField">Custom field to group trace table columns by</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void ProcessLaborData(FullWorkspace workspace, string currentLevel, ICollection<string> additionalLevels, ICollection<ResourceTypeDto> resourceTypes, TraceTableBoeDataGroup parent, bool includeYearlyData, string groupingField, string customGroupingField)
		{
			if (string.IsNullOrEmpty(currentLevel))
			{
				// spreads are now filtered
				ICollection<ResourceSpreadDto> spreads = resourceTypes.SelectMany(x => x.LaborSpreads).ToList();
				parent.TotalValue = spreads.Sum(x => x.LaborSpreadValue);
				parent.SpreadPrecision = resourceTypes.FirstOrDefault()?.SpreadType == SpreadType.Cost ? workspace.CostDecimalPrecision : workspace.DecimalPrecision;

				if (string.IsNullOrEmpty(customGroupingField))
				{
					switch (groupingField)
					{
						case "CalendarYear":
							for (int year = workspace.StartDate.Value.Year; year <= workspace.EndDate.Value.Year; year++)
							{
								parent.SpreadValuesForGroup.Add(year.ToString(), spreads.Where(x => x.LaborSpreadDate.Year == year).Sum(x => x.LaborSpreadValue));
							}
							break;
						case "CLIN":
							foreach (var clin in workspace.Clins)
							{
								parent.SpreadValuesForGroup.Add(clin.ClinString, resourceTypes.Where(r => r.CLINID == clin.Id).SelectMany(s => s.LaborSpreads).Sum(x => x.LaborSpreadValue)); 
							}
							break;
						case "WBS":
							foreach (var wbs in workspace.WbsElements)
							{
                                parent.SpreadValuesForGroup.Add(wbs.WbsString, resourceTypes.Where(r => r.WBSID == wbs.Id).SelectMany(s => s.LaborSpreads).Sum(x => x.LaborSpreadValue));
                            }
                            break;
                        case "Blank":
                            break;
                        default:
                            break;
                    }
                }
                else
				{
                    CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName.Equals(customGroupingField));

					if (customField != null)
					{
						ICollection<string> customFieldValues = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x => x.CustomFieldValueDescription).Distinct().ToList();

						foreach (string customFieldValue in customFieldValues)
						{
							// figure out which resources should be selected; the complication is that the custom field can be on resource, task or BOE levels, so we have to check all 3
							List<ResourceTypeDto> resourcesForCustomField = resourceTypes
								.Where(x => x.CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									|| workspace.TaskElements.First(t => t.Id == x.TaskElementId).CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									|| workspace.Boes.First(b => b.Id == x.BoeID).CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									).ToList();

							parent.SpreadValuesForGroup.Add(customFieldValue, resourcesForCustomField.SelectMany(s => s.LaborSpreads).Sum(x => x.LaborSpreadValue));
						}
					}
					else throw new ArgumentNullException(customGroupingField, "Custom field not found");
                }
			}
			else
			{
				string nextLevel = additionalLevels.FirstOrDefault();
				ICollection<string> nextAdditionalLevels = additionalLevels.Skip(1).ToList();

				// TODO - see if we need to order these (all categories)

				if (currentLevel.Equals(SummaryFieldType.CLINNum.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int? clinId in resourceTypes.Select(x => x.CLINID).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.Clins.FirstOrDefault(x => x.Id == clinId)?.ClinNumber ?? CommonConstants.Unassigned_CLIN_Display_Text
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.CLINID == clinId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel.Equals(SummaryFieldType.WBSNum.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					IList<int?> wbsIdList = resourceTypes.Select(x => x.WBSID).Distinct().ToList();
					IList<int?> wbsElementsNoMultiWbsList = workspace.WbsElementsNoMultiWbs.Where(x => wbsIdList.Contains(x.Id)).Select(x => (int?)x.Id).ToList();

					if (wbsIdList.Any(x => x == null))
					{
						wbsElementsNoMultiWbsList.Add(null);
					}

					foreach (int? wbsId in wbsElementsNoMultiWbsList)
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.WbsElements.FirstOrDefault(x => x.Id == wbsId)?.WbsNumber ?? CommonConstants.Unassigned_WBS_Display_Text
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.WBSID == wbsId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel.Equals(SummaryFieldType.ResourceOrActivityId.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int? resourceId in resourceTypes.Select(x => x.ResourceID).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == resourceId)?.ResourceName ?? "NO RESOURCE ID"
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.ResourceID == resourceId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel.Equals(SummaryFieldType.ResourceDescription.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int? resourceId in resourceTypes.Select(x => x.ResourceID).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == resourceId)?.ResourceDesc ?? "NO RESOURCE DESCRIPTION"
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.ResourceID == resourceId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel.Equals(SummaryFieldType.PerformingOrgId.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int? perfOrgId in resourceTypes.Select(x => x.PerformingOrgID).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.PerformingOrgsUsedInBoes.FirstOrDefault(x => x.Id == perfOrgId)?.PerformingOrgName ?? "NO PERF ORG ID"
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.PerformingOrgID == perfOrgId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}

				}
				else if (currentLevel.Equals(SummaryFieldType.TaskTitle.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int taskId in resourceTypes.Select(x => x.TaskElementId).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.TaskElements.FirstOrDefault(x => x.Id == taskId)?.TaskTitle ?? "NO TASK TITLE"
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.TaskElementId == taskId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel.Equals(SummaryFieldType.BOETitle.GetDescription(), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (int boeId in resourceTypes.Select(x => x.BoeID).Distinct())
					{
						TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
						{
							SummaryField = currentLevel.GetDescription(),
							SummaryFieldValue = workspace.Boes.FirstOrDefault(x => x.Id == boeId)?.Title ?? "NO BOE TITLE"
						};

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.BoeID == boeId).ToList(), newChild, includeYearlyData, groupingField, customGroupingField);
						parent.ChildData.Add(newChild);
					}
				}
				else
				{
					// Check custom fields if summary field does not match any of the previous Summary Field Types
					CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName.Equals(currentLevel, StringComparison.CurrentCultureIgnoreCase));

					if (customField != null)
					{
						ICollection<string> customFieldValues = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x => x.CustomFieldValueDescription).Distinct().ToList();
						List<int> processedResourceIds = new List<int>();

						foreach (string customFieldValue in customFieldValues)
						{
							// figure out which resources should be selected; the complication is that the custom field can be on resource, task or BOE levels, so we have to check all 3
							List<ResourceTypeDto> resourcesForCustomField = resourceTypes
								.Where(x => x.CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									|| workspace.TaskElements.First(t => t.Id == x.TaskElementId).CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									|| workspace.Boes.First(b => b.Id == x.BoeID).CustomFieldValueContainers.Any(z => z.CustomFieldID == customField.Id && z.OpenEndedValue.Equals(customFieldValue, StringComparison.CurrentCultureIgnoreCase))
									).ToList();

							processedResourceIds.AddRange(resourcesForCustomField.Select(x => x.Id));

							ProcessCustomFieldValue(workspace, currentLevel, parent, includeYearlyData, nextLevel, nextAdditionalLevels, customFieldValue, resourcesForCustomField, groupingField, customGroupingField);
						}

						// finally we have to also look for blank custom field values; since this isn't stored,
						// we'll consider all resource types that didn't get processed up to this point as not having a value for the specific CF
						ProcessCustomFieldValue(workspace, currentLevel, parent, includeYearlyData, nextLevel, nextAdditionalLevels, CommonConstants.NO_CUSTOM_FIELD_VALUE, 
													resourceTypes.Where(x => !processedResourceIds.Contains(x.Id)).ToList(), groupingField, customGroupingField);
					}
				}
			}
		}

        /// <summary>
        /// Process Custom Field Values for Trace Table export
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <param name="currentLevel">Current Level</param>
        /// <param name="parent">Parent</param>
        /// <param name="includeYearlyData">Should yearly data be included</param>
        /// <param name="nextLevel">Next Level</param>
        /// <param name="nextAdditionalLevels">Next Additional Levels</param>
        /// <param name="customFieldValue">Custom Field Value</param>
        /// <param name="resourcesForCustomField">Resources for Custom Field</param>
        /// <param name="groupingField">Field to group trace table columns by</param>
		/// <param name="customGroupingField">Custom field to group trace table columns by</param>
        private void ProcessCustomFieldValue(FullWorkspace workspace, string currentLevel, TraceTableBoeDataGroup parent, bool includeYearlyData, string nextLevel, ICollection<string> nextAdditionalLevels, string customFieldValue, List<ResourceTypeDto> resourcesForCustomField, string groupingField, string customGroupingField)
		{
			// only run the value if it's been used, to avoid a bunch of empty rows
			if (resourcesForCustomField.Any())
			{
				TraceTableBoeDataGroup newChild = new TraceTableBoeDataGroup()
				{
					SummaryField = currentLevel.GetDescription(),
					SummaryFieldValue = customFieldValue
				};

				ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourcesForCustomField, newChild, includeYearlyData, groupingField, customGroupingField);
				parent.ChildData.Add(newChild);
			}
		}
	}
}
