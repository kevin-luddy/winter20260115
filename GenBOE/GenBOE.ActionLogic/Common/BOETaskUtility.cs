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
		/// <param name="ws">Full workspace.</param>
		/// <param name="taskElement">Task element</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		public static bool IsUsingTMRates(FullWorkspace ws, BoeTaskElementDTO taskElement)
		{
			if (taskElement == null)
			{
				throw new ArgumentNullException(nameof(taskElement));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			List<int> resourceIds = taskElement.taskElementLabors
				.Where(lt => lt.ResourceID.HasValue)
				.Select(lt => lt.ResourceID.Value)
				.Distinct()
				.ToList();

			List<int> businessResourceCodeIds = taskElement.taskElementLabors
				.Where(lt => lt.BusinessResourceCodeID.HasValue)
				.Select(lt => lt.BusinessResourceCodeID.Value)
				.Distinct()
				.ToList();

			if (!resourceIds.Any() && !businessResourceCodeIds.Any())
			{
				return false;
			}

			HashSet<int> allResourceIds = resourceIds.Concat(businessResourceCodeIds).ToHashSet();

			List<ResourceDTO> resources = ws.ResourcesUsedInWsBoes
				.Where(r => allResourceIds.Contains(r.Id))
				.ToList();

			return CompareResources(ws.TMResourceRatesForWorkspace, resources);
		}

		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="tmResourceRates">The T&M resource rates.</param>
		/// <param name="resources">The resources.</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		private static bool CompareResources(IReadOnlyCollection<TMResourceRateDTO> tmResourceRates, ICollection<ResourceDTO> resources)
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