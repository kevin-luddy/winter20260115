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
	using GenBOE.Dtos;
	using IES.Common;
	using MoreLinq;

	/// <summary>
	/// UCOT Specific Utility Methods
	/// </summary>
	public static class UCOTUtility
	{
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

			if (Utilities.ShowUCOTForWorkspace(workspaceCreationDate, workspaceShortname) && rateType == RateType.Hours  && elementOfCost == ElementOfCostType.LMLabor && taskMoqTypes.Count == 1)
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
						decimal absUcotTotal = Math.Abs(Utilities.AdjustPrecision(ucotTotal, decimalPrecision));

						// Calculate UCOT values for spreads
						IDictionary<DateTime, decimal> ucotSpreads = new Dictionary<DateTime, decimal>();
						foreach (KeyValuePair<DateTime, decimal> spread in spreadsToApplyUcot.OrderBy(x => x.Key))
						{
							ucotSpreads.Add(spread.Key, Utilities.AdjustPrecision(spread.Value * ucotFactor / 100m, decimalPrecision));
						}

						// Get smoothed curve values
						decimal[] smoothedSpreadValues = SpreadCurve.Smooth(absUcotTotal, ucotSpreads.Select(x => x.Value).ToArray(), 0, ucotSpreads.Count, decimalPrecision);

						if (ucotTotal < 0)
						{
							// if UCOT is negative, then change the sign
							smoothedSpreadValues = SpreadCurve.ChangeSign(smoothedSpreadValues, 0, smoothedSpreadValues.Length);
						}

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

	}
}
