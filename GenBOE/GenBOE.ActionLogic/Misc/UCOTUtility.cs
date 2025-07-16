// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Misc
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using MoreLinq;

	/// <summary>
	/// UCOT Specific Utility Methods
	/// </summary>
	public static class UCOTUtility
	{
		/// <summary>
		/// Get UCOT Hours Total for the Task Element Collection
		/// </summary>
		/// <param name="fullWorkspace">Full Workspace to pull all necessary data for UCOT data manipulation</param>
		/// <param name="taskElements">Specific Tasks to Iterate through to get UCOT Totals</param>
		public static decimal GetTaskElementsUCOTHours(FullWorkspace fullWorkspace, IReadOnlyCollection<BoeTaskElementDTO> taskElements)
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
		/// <param name="taskElements">Specific Tasks to Iterate through to get UCOT Totals</param>
		public static void SetTaskElementsUCOTHours(FullWorkspace fullWorkspace, IReadOnlyCollection<BoeTaskElementDTO> taskElements)
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

				taskElements.ForEach(taskElement =>
				{
					decimal ucotTotal = CalculateTaskUCOTHours(taskElement, moqTypeSelectionDictionary, resourceDictionary);

					taskElement.UCOTHours = Utilities.AdjustPrecision(ucotTotal * fullWorkspace.UCOTFactor / 100m, fullWorkspace.ResourceDecimalPrecision);
					taskElement.TotalHoursWithUCOT = taskElement.UCOTHours + taskElement.TotalHours;
				});
			}
		}

		/// <summary>
		/// Get the UCOT spreads for the given Labor Spreads
		/// </summary>
		/// <param name="decimalPrecision">decimal precision</param>
		/// <param name="ucotFactor">Ucot factor</param>
		/// <param name="workspaceCreationDate">Workspace creation date</param>
		/// <param name="laborSpreads">Labor/Resource Spreads</param>
		/// <param name="workspaceShortname">Workspace Shortname</param>
		/// <param name="elementOfCost">Element of Cost for the Labor</param>
		/// <param name="boeTaskElementId">Task Element Id</param>
		/// <param name="workspaceMOQTypes">Workspace MOQ Types</param>
		/// <returns>Smoothed UCOT spreads for the Labor</returns>
		public static IDictionary<DateTime, decimal> GetUcotSpreads(DateTime? workspaceCreationDate, string workspaceShortname, int decimalPrecision, decimal ucotFactor,
			ICollection<ResourceSpreadDto> laborSpreads, ElementOfCostType elementOfCost, ICollection<MoqTypeSelection> workspaceMOQTypes, int boeTaskElementId, RateType rateType)
		{
			if (workspaceMOQTypes == null)
			{
				throw new ArgumentNullException(nameof(workspaceMOQTypes));
			}

			IDictionary<DateTime, decimal> smoothedUcotSpreads = new Dictionary<DateTime, decimal>();
			ICollection<MOQType> taskMoqTypes = workspaceMOQTypes.Where(z => z.TaskId == boeTaskElementId).Select(m => m.SelectedMOQType).ToList();

			if (Utilities.ShowUCOTForWorkspace(workspaceCreationDate, workspaceShortname) && rateType == RateType.Hours && elementOfCost == ElementOfCostType.LMLabor && taskMoqTypes.Count == 1)
			{
				MOQType taskMoqType = taskMoqTypes.First();
				if (taskMoqType == MOQType.Historical || taskMoqType == MOQType.Comparative || taskMoqType == MOQType.AnalogousRelationships)
				{
					// Get spreads on/after 1LMX start to see if UCOT needs to be applied
					IDictionary<DateTime, decimal> spreadsToApplyUcot = laborSpreads.Where(x => x.LaborSpreadDate >= Utilities.OneLmxStartDate)
						.ToDictionary(x => x.LaborSpreadDate, x => x.LaborSpreadValue);

					if (spreadsToApplyUcot.Any())
					{
						// Get UCOT Total
						decimal ucotTotal = spreadsToApplyUcot.Sum(x => x.Value) * (ucotFactor / 100m);
						ucotTotal = Utilities.AdjustPrecision(ucotTotal, decimalPrecision);

						// Calculate UCOT values for spreads
						IDictionary<DateTime, decimal> ucotSpreads = new Dictionary<DateTime, decimal>();
						foreach (KeyValuePair<DateTime, decimal> spread in spreadsToApplyUcot.OrderBy(x => x.Key))
						{
							ucotSpreads.Add(spread.Key, Utilities.AdjustPrecision(spread.Value * ucotFactor / 100m, decimalPrecision));
						}

						// Get smoothed curve values
						decimal[] smoothedSpreadValues = SpreadCurve.Smooth(ucotTotal, ucotSpreads.Select(x => x.Value).ToArray(), 0, ucotSpreads.Count, decimalPrecision);

						// Add the smoothed values to the dictionary to apply to spreads later
						for (int i = 0; i < ucotSpreads.Count; i++)
						{
							smoothedUcotSpreads.Add(ucotSpreads.ElementAt(i).Key, smoothedSpreadValues[i]);
						}
					}
				}
			}

			return smoothedUcotSpreads;
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
