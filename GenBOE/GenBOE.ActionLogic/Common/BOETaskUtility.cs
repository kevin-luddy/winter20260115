// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
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
		/// <param name="workspaceId">Workspace Id.</param>
		/// <param name="laborTypes">Boe Task Element.</param>
		/// <param name="retriever">Retriever object</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		public static bool IsUsingTMRates(FullWorkspace ws, IReadOnlyCollection<TMResourceRateDTO> tmResourceRates, IReadOnlyCollection<BoeTaskElementDTO> laborTypes)
		{
			if (laborTypes == null)
			{
				throw new ArgumentNullException(nameof(laborTypes));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (tmResourceRates == null)
			{
				throw new ArgumentNullException(nameof(tmResourceRates));
			}

			List<int> resourceIds = laborTypes
				.SelectMany(te => te.taskElementLabors)
				.Where(lt => lt.ResourceID.HasValue)
				.Select(lt => lt.ResourceID.Value)
				.Distinct()
				.ToList();

			List<int> businessResourceCodeIds = laborTypes
				.SelectMany(te => te.taskElementLabors)
				.Where(lt => lt.BusinessResourceCodeID.HasValue)
				.Select(lt => lt.BusinessResourceCodeID.Value)
				.Distinct()
				.ToList();

			HashSet<int> allResourceIds = resourceIds.Concat(businessResourceCodeIds).ToHashSet();

			List<ResourceDTO> resources = ws.ResourcesUsedInWsBoes
				.Where(r => allResourceIds.Contains(r.Id))
				.ToList();

			return IsUsingTMRates(tmResourceRates, resources);
		}

		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="tmResourceRates">The T&M resource rates.</param>
		/// <param name="resources">The resources.</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		public static bool IsUsingTMRates(IReadOnlyCollection<TMResourceRateDTO> tmResourceRates, ICollection<ResourceDTO> resources)
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