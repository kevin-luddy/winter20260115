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
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

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

			ProcessLaborData(workspace, settingsData.SummaryFields.FirstOrDefault(), settingsData.SummaryFields.Skip(1).ToList(),
				taskElementLabors, boeData, settingsData.ShowYears);

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void ProcessLaborData(FullWorkspace workspace, string currentLevel, ICollection<string> additionalLevels, ICollection<ResourceTypeDto> resourceTypes, TraceTableBoeData parent, bool includeYearlyData)
		{
			if (string.IsNullOrEmpty(currentLevel))
			{
				// spreads are now filtered
				ICollection<ResourceSpreadDto> spreads = resourceTypes.SelectMany(x => x.LaborSpreads).ToList();
				parent.TotalValue = spreads.Sum(x => x.LaborSpreadValue);

				if (includeYearlyData)
				{
					ICollection<int> years = spreads.Select(x => x.LaborSpreadDate.Year).Distinct().ToList();

					foreach (int year in years)
					{
						parent.SpreadValuesForYear.Add(year, spreads.Where(x => x.LaborSpreadDate.Year == year).Sum(x => x.LaborSpreadValue));
					}
				}
			}
			else
			{
				string nextLevel = additionalLevels.FirstOrDefault();
				ICollection<string> nextAdditionalLevels = additionalLevels.Skip(1).ToList();

				if (currentLevel == SummaryFieldType.CLINNum.GetDescription())
				{
					foreach (int? clinId in resourceTypes.Select(x => x.CLINID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						if (clinId is null)
						{
							newChild.SummaryFieldValue = CommonConstants.Unassigned_CLIN_Display_Text;
						}
						else
						{
							FullClin clin = workspace.Clins.FirstOrDefault(x => x.Id == clinId);
							if (clin != null)
							{
								newChild.SummaryFieldValue = clin.ClinNumber;
							}
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.CLINID == clinId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel == SummaryFieldType.WBSNum.GetDescription())
				{
					foreach (int? wbsId in resourceTypes.Select(x => x.WBSID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						if (wbsId is null)
						{
							newChild.SummaryFieldValue = CommonConstants.Unassigned_WBS_Display_Text;
						}
						else
						{
							FullWbs wbs = workspace.WbsElements.FirstOrDefault(x => x.Id == wbsId);
							if (wbs != null)
							{
								newChild.SummaryFieldValue = wbs.WbsNumber;
							}
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.WBSID == wbsId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel == SummaryFieldType.ResourceOrActivityId.GetDescription())
				{
					foreach (int? resourceId in resourceTypes.Select(x => x.ResourceID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						if (resourceId is null)
						{
							newChild.SummaryFieldValue = "NO RESOURCE ID";
						}
						else
						{
							ResourceDTO resource = workspace.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == resourceId);
							if (resource != null)
							{
								newChild.SummaryFieldValue = resource.ResourceName;
							}
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.ResourceID == resourceId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel == SummaryFieldType.ResourceDescription.GetDescription())
				{
					foreach (int? resourceId in resourceTypes.Select(x => x.ResourceID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						if (resourceId is null)
						{
							newChild.SummaryFieldValue = "NO RESOURCE DESCRIPTION";
						}
						else
						{
							ResourceDTO resource = workspace.ResourcesUsedInWsBoes.FirstOrDefault(x => x.Id == resourceId);
							if (resource != null)
							{
								newChild.SummaryFieldValue = resource.ResourceDesc;
							}
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.ResourceID == resourceId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel == SummaryFieldType.PerformingOrgId.GetDescription())
				{
					foreach (int? perfOrgId in resourceTypes.Select(x => x.PerformingOrgID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						if (perfOrgId is null)
						{
							newChild.SummaryFieldValue = "NO PERF ORG ID";
						}
						else
						{
							PerformingOrgDTO perfOrg = workspace.PerformingOrgsUsedInBoes.FirstOrDefault(x => x.Id == perfOrgId);
							if (perfOrg != null)
							{
								newChild.SummaryFieldValue = perfOrg.PerformingOrgName;
							}
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.PerformingOrgID == perfOrgId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}

				}
				else if (currentLevel == SummaryFieldType.TaskDescription.GetDescription())
				{
					foreach (int taskId in resourceTypes.Select(x => x.TaskElementId).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						BoeTaskElementDTO task = workspace.TaskElements.FirstOrDefault(x => x.Id == taskId);
						if (task != null)
						{
							newChild.SummaryFieldValue = task.TaskTitle;
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.TaskElementId == taskId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else if (currentLevel == SummaryFieldType.BOETitle.GetDescription())
				{
					foreach (int boeId in resourceTypes.Select(x => x.BoeID).Distinct())
					{
						TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };

						FullBoe boe = workspace.Boes.FirstOrDefault(x => x.Id == boeId);
						if (boe != null)
						{
							newChild.SummaryFieldValue = boe.Title;
						}

						ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.BoeID == boeId).ToList(), newChild, includeYearlyData);
						parent.ChildData.Add(newChild);
					}
				}
				else
				{
					// Check custom fields if summary field does not match any of the previous Summary Field Types
					CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(x => x.CustomFieldName == currentLevel);

					if (customField != null)
					{
						foreach (var customFieldValue in resourceTypes.SelectMany(x => x.CustomFieldValueContainers).Where(x => x.CustomFieldID == customField.Id).Select(x => new { x.CustomFieldValueID, x.OpenEndedValue }).Distinct())
						{
							TraceTableBoeData newChild = new TraceTableBoeData() { SummaryField = currentLevel.GetDescription() };
							newChild.SummaryFieldValue = customFieldValue.OpenEndedValue; 

							ProcessLaborData(workspace, nextLevel, nextAdditionalLevels, resourceTypes.Where(x => x.CustomFieldValueContainers.Any(y => y.CustomFieldID == customField.Id && y.CustomFieldValueID == customFieldValue.CustomFieldValueID)).ToList(), newChild, includeYearlyData);
							parent.ChildData.Add(newChild);
						}
					}
				}
			}
		}
	}
}
