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
				throw new ArgumentNullException(nameof (retriever));
			}

			bool isUsingTMRatesInTask = false;

			// Check if the company mode is Space Systems as T&M rates only apply to space.
			if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.SpaceSystems)
			{
				ICollection<TMResourceRateDTO> tmResourceRates = retriever.GetTMResourceRates(workspaceId);

				if (tmResourceRates.Any() && laborTypes != null)
				{
					// Get the resource IDs and business resource code IDs
					List<int> resourceIds = laborTypes.Where(lt => lt.ResourceID.HasValue).Select(lt => lt.ResourceID.Value).Distinct().ToList();
					List<int> businessResourceCodeIds = laborTypes.Where(lt => lt.BusinessResourceCodeID.HasValue).Select(lt => lt.BusinessResourceCodeID.Value).Distinct().ToList();

					// Get the resources and business resource codes from the database
					HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(retriever.GetResourcesByIds(resourceIds));
					HashSet<ResourceDTO> businessResourcesFromDb = new HashSet<ResourceDTO>(retriever.GetResourcesByIds(businessResourceCodeIds));

					// Convert ResourceTypeDto to LaborTypeDataModelView for comparison
					ICollection<LaborTypeDataModelView> laborTypeDataModelViews = laborTypes
						.Select(lt => new LaborTypeDataModelView(lt,
							lt.ResourceID.HasValue ? resourcesFromDb.First(x => x.Id == lt.ResourceID.Value) : new ResourceDTO(),
							lt.BusinessResourceCodeID.HasValue ? businessResourcesFromDb.First(x => x.Id == lt.BusinessResourceCodeID.Value) : new ResourceDTO(),
							new PerformingOrgDTO(),
							0, false))
						.ToList();

					foreach (LaborTypeDataModelView laborType in laborTypeDataModelViews)
					{
						// Check if there's a matching T&M resource rate
						TMResourceRateDTO matchingResourceRate = tmResourceRates.FirstOrDefault(r => r.ResourceName == laborType.ResourceName || r.ResourceName == laborType.BusinessResourceCodeName);

						// If a match is found then T&M resource rates are being used.
						if (matchingResourceRate != null)
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