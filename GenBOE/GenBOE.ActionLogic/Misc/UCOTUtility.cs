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
	using IES.Common;
	using IES.Common.classes;
	using MoreLinq;
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
		/// <param name="taskElements">Task Elements to ADD the UCOT Hours too</param>
		/// <param name="moqTypeSelections">Workspace MOQ Type Selections</param>
		/// <param name="resourcesUsedInWsBoes">Resources Used in Workspace BOES</param>
		/// <param name="ucotFactor">UCOT Factor for the Workspace</param>
		/// <param name="resourceDecimalPrecision">Resource Decimal Precision</param>
		public static decimal GetTaskElementsUCOTData(IReadOnlyCollection<BoeTaskElementDTO> taskElements, IReadOnlyCollection<MoqTypeSelection> moqTypeSelections, IReadOnlyCollection<ResourceDTO> resourcesUsedInWsBoes, decimal ucotFactor, int? resourceDecimalPrecision)
		{
			decimal ucotHoursTotal = 0m;

			// We will only do calculations when in SPACE
			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary = moqTypeSelections.GroupBy(m => m.TaskId).ToDictionary(d => d.Key);
				IDictionary<int, ResourceDTO> resourceDictionary = resourcesUsedInWsBoes.ToDictionary(r => r.Id);

				taskElements.ForEach(t =>
				{
					decimal ucotTotal = TaskUCOTData(t, moqTypeSelectionDictionary, resourceDictionary);

					t.UCOTHours = Utilities.AdjustPrecision(ucotTotal * ucotFactor / 100m, resourceDecimalPrecision);
					ucotHoursTotal += t.UCOTHours.HasValue ? t.UCOTHours.Value : 0;
				});
			}

			return ucotHoursTotal;
		}

		/// <summary>
		/// Set UCOT Hours Total for the Task Element Collection (this method sets it on the each task)
		/// </summary>
		/// <param name="taskElements">Task Elements to ADD the UCOT Hours too</param>
		/// <param name="moqTypeSelections">Workspace MOQ Type Selections</param>
		/// <param name="resourcesUsedInWsBoes">Resources Used in Workspace BOES</param>
		/// <param name="ucotFactor">UCOT Factor for the Workspace</param>
		/// <param name="resourceDecimalPrecision">Resource Decimal Precision</param>
		public static void SetTaskElementsUCOTData(IReadOnlyCollection<BoeTaskElementDTO> taskElements, IReadOnlyCollection<MoqTypeSelection> moqTypeSelections, IReadOnlyCollection<ResourceDTO> resourcesUsedInWsBoes, decimal ucotFactor, int? resourceDecimalPrecision)
		{
			// We will only do calculations when in SPACE
			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary = moqTypeSelections.GroupBy(m => m.TaskId).ToDictionary(d => d.Key);
				IDictionary<int, ResourceDTO> resourceDictionary = resourcesUsedInWsBoes.ToDictionary(r => r.Id);

				taskElements.ForEach(t =>
				{
					decimal ucotTotal = TaskUCOTData(t, moqTypeSelectionDictionary, resourceDictionary);

					t.UCOTHours = Utilities.AdjustPrecision(ucotTotal * ucotFactor / 100m, resourceDecimalPrecision);
					t.TotalHoursWithUCOT = t.UCOTHours + t.TotalHours;
				});
			}
		}

		/// <summary>
		/// Private Method to calculate individual Task Element UCOT Data
		/// </summary>
		/// <param name="t">Task Element DTO</param>
		/// <param name="moqTypeSelectionDictionary">MOQ Type Selection Dictionary</param>
		/// <param name="resourceDictionary">Resource Dictionary</param>
		/// <returns>UCOT Total for the Task</returns>
		private static decimal TaskUCOTData(BoeTaskElementDTO t, Dictionary<int, IGrouping<int, MoqTypeSelection>> moqTypeSelectionDictionary, IDictionary<int, ResourceDTO> resourceDictionary)
		{
			decimal ucotTotal = 0m;
			IGrouping<int, MoqTypeSelection> moqGroup = moqTypeSelectionDictionary[t.Id];

			if (moqGroup.Count() == 1)
			{
				MoqTypeSelection moqType = moqGroup.First();

				if (moqType.SelectedMOQType == MOQType.AnalogousRelationships || moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.Comparative)
				{
					t.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours).ForEach(labor =>
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
