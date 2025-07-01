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
		/// Set UCOT Hours Total for the Task Element
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
				IDictionary<int, MoqTypeSelection> moqTypeSelectionDictionary = moqTypeSelections.ToDictionary(m => m.TaskId);
				IDictionary<int, ResourceDTO> resourceDictionary = resourcesUsedInWsBoes.ToDictionary(r => r.Id);

				taskElements.ForEach(t =>
				{
					decimal ucotTotal = 0m;
					MoqTypeSelection moqType = moqTypeSelectionDictionary[t.Id];

					if (moqType.SelectedMOQType == MOQType.AnalogousRelationships || moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.Comparative)
					{
						t.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours).ForEach(labor =>
						{
							if (labor.ResourceID.HasValue)
							{
								ResourceDTO resource = resourceDictionary[labor.ResourceID.Value];

								if (resource != null && resource.ElementOfCost == ElementOfCostType.LMLabor)
								{
									ucotTotal += labor.LaborSpreads
										.Where(s => s.LaborSpreadDate >= Utilities.OneLmxStartDate)
										.Sum(spread => spread.LaborSpreadValue);
								}
							}
						});
					}

					t.UCOTHours = Utilities.AdjustPrecision(ucotTotal * ucotFactor / 100m, resourceDecimalPrecision);
					t.TotalHoursWithUCOT = t.UCOTHours + t.TotalHours;
				});
			}
		}
	}
}
