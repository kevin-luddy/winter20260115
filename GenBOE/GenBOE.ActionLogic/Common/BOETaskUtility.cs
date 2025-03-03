// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common.classes;
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
		public static bool IsUsingTMRates(int workspaceId, ICollection<ResourceTypeDto> laborTypes, IRetriever retriever)
		{
			if (laborTypes == null)
			{
				throw new ArgumentNullException(nameof(laborTypes));
			}

			if (retriever == null)
			{
				throw new ArgumentNullException(nameof(retriever));
			}

			bool isUsingTMRatesInTask = false;

			// Check if the company mode is Space Systems as T&M rates only apply to space.
			if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.SpaceSystems)
			{
				ICollection<TMResourceRateDTO> tmResourceRates = retriever.GetTMResourceRates(workspaceId);

				if (tmResourceRates.Any())
				{
					// Get the resource IDs and business resource code IDs
					List<int> resourceIds = laborTypes.Where(lt => lt.ResourceID.HasValue).Select(lt => lt.ResourceID.Value).Distinct().ToList();
					List<int> businessResourceCodeIds = laborTypes.Where(lt => lt.BusinessResourceCodeID.HasValue).Select(lt => lt.BusinessResourceCodeID.Value).Distinct().ToList();

					// Get the resources and business resource codes from the database
					HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(retriever.GetResourcesByIds(resourceIds.Concat(businessResourceCodeIds).ToList()));

					// Check if there's a matching T&M resource rate for any resource
					foreach (TMResourceRateDTO tmResourceRate in tmResourceRates)
					{
						if (resourcesFromDb.Any(r => r.ResourceName == tmResourceRate.ResourceName))
						{
							isUsingTMRatesInTask = true;
							break;
						}
					}
				}
			}

			return isUsingTMRatesInTask;
		}
	}
}