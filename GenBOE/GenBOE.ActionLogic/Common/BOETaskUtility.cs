// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.Dtos;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Class utilized to run some general validation on a BOE task.
	/// </summary>
	public static class BOETaskUtility
	{
		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="tmResourceRates">The T&M resource rates.</param>
		/// <param name="resources">The resources.</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		public static bool IsUsingTMRates(ICollection<TMResourceRateDTO> tmResourceRates, ICollection<ResourceDTO> resources)
		{
			if (tmResourceRates == null)
			{
				throw new ArgumentNullException(nameof(tmResourceRates));
			}

			if (resources == null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (tmResourceRates.Any() && resources.Any())
			{
				foreach (TMResourceRateDTO tmResourceRate in tmResourceRates)
				{
					if (resources.Any(r => r.ResourceName == tmResourceRate.ResourceName))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}