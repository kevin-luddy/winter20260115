// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Misc
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using MoreLinq;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// UCOT Specific Utility Methods
	/// </summary>
	public static class UCOTUtility
	{
		/// <summary>
		/// Get UCOT Hours Total for the Task Element Collection
		/// </summary>
		/// <param name="fullWorkspace">Full Workspace to pull all necessary data for UCOT data manipulation</param>
		/// <param name="taskElementsToIterate">Specific Tasks to Iterate through to get UCOT Totals</param>
		public static decimal GetTaskElementsUCOTHours(FullWorkspace fullWorkspace, IReadOnlyCollection<BoeTaskElementDTO> taskElementsToIterate)
		{
			if (fullWorkspace == null)
			{
				throw new ArgumentNullException(nameof(fullWorkspace));
			}

			decimal ucotHoursTotal = 0m;

			// We will only do calculations when in SPACE
			if (Utilities.ShowUCOTForWorkspace(fullWorkspace.CreationDate, fullWorkspace.Shortname))
			{
				Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary = fullWorkspace.MoqTypeSelections.GroupBy(m => m.TaskId).ToDictionary(d => d.Key);
				IDictionary<int, ResourceDTO> resourceDictionary = fullWorkspace.ResourcesUsedInWsBoes.ToDictionary(r => r.Id);
				IReadOnlyCollection<BoeTaskElementDTO> taskElements = taskElementsToIterate.Count() > 0 ? taskElementsToIterate : fullWorkspace.TaskElements;

				taskElements.ForEach(taskElement =>
				{
					decimal ucotTotal = CalculateTaskUCOTHours(taskElement, moqTypeSelectionDictionary, resourceDictionary);

					taskElement.UCOTHours = Utilities.AdjustPrecision(ucotTotal * fullWorkspace.UCOTFactor / 100m, fullWorkspace.ResourceDecimalPrecision);
					ucotHoursTotal += taskElement.UCOTHours.HasValue ? taskElement.UCOTHours.Value : 0;
				});
			}

			return ucotHoursTotal;
		}

		/// <summary>
		/// Set UCOT Hours Total for the Task Element Collection (this method sets it on the individual task)
		/// </summary>
		/// <param name="fullWorkspace">Full Workspace to pull all necessary data for UCOT data manipulation</param>
		/// <param name="taskElementsToIterate">Specific Tasks to Iterate through to get UCOT Totals</param>
		public static void SetTaskElementsUCOTHours(FullWorkspace fullWorkspace, IReadOnlyCollection<BoeTaskElementDTO> taskElementsToIterate)
		{
			if (fullWorkspace == null)
			{
				throw new ArgumentNullException(nameof(fullWorkspace));
			}

			// We will only do calculations when in SPACE
			if (Utilities.ShowUCOTForWorkspace(fullWorkspace.CreationDate, fullWorkspace.Shortname))
			{
				Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary = fullWorkspace.MoqTypeSelections.GroupBy(m => m.TaskId).ToDictionary(d => d.Key);
				IDictionary<int, ResourceDTO> resourceDictionary = fullWorkspace.ResourcesUsedInWsBoes.ToDictionary(r => r.Id);
				IReadOnlyCollection<BoeTaskElementDTO> taskElements = taskElementsToIterate.Count() > 0 ? taskElementsToIterate : fullWorkspace.TaskElements;

				taskElements.ForEach(taskElement =>
				{
					decimal ucotTotal = CalculateTaskUCOTHours(taskElement, moqTypeSelectionDictionary, resourceDictionary);

					taskElement.UCOTHours = Utilities.AdjustPrecision(ucotTotal * fullWorkspace.UCOTFactor / 100m, fullWorkspace.ResourceDecimalPrecision);
					taskElement.TotalHoursWithUCOT = taskElement.UCOTHours + taskElement.TotalHours;
				});
			}
		}

		/// <summary>
		/// Private Method to calculate individual Task Element UCOT Data
		/// </summary>
		/// <param name="taskElement">Task Element DTO</param>
		/// <param name="moqTypeSelectionDictionary">MOQ Type Selection Dictionary</param>
		/// <param name="resourceDictionary">Resource Dictionary</param>
		/// <returns>UCOT Total for the Task</returns>
		private static decimal CalculateTaskUCOTHours(BoeTaskElementDTO taskElement, Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary, IDictionary<int, ResourceDTO> resourceDictionary)
		{
			decimal ucotTotal = 0m;

			if (!moqTypeSelectionDictionary.ContainsKey(taskElement.Id))
			{
				return ucotTotal;
			}

			IGrouping<int, MoqTypeSelection> moqGroup = moqTypeSelectionDictionary[taskElement.Id];

			if (moqGroup.Count() == 1)
			{
				MoqTypeSelection moqType = moqGroup.First();

				if (moqType.SelectedMOQType == MOQType.AnalogousRelationships || moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.Comparative)
				{
					taskElement.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours).ForEach(labor =>
					{
						if (labor.BusinessResourceCodeID.HasValue)
						{
							ResourceDTO resource = resourceDictionary[labor.BusinessResourceCodeID.Value];

							if (resource != null && resource.ElementOfCost == ElementOfCostType.LMLabor)
							{
								ucotTotal += labor.LaborSpreads
									.Where(s => s.LaborSpreadDate >= Utilities.OneLmxStartDate)
									.Sum(spread => spread.LaborSpreadValue);
							}
						}
					});
				}
			}

			return ucotTotal;
		}

	}


}
