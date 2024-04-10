// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO
{
	using GenBOE.ActionLogic.Common;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public static class ImportExportUtilities
	{
		/// <summary>
		/// Returns Resources / Business Resource Codes based on Company mode and 1LMX or Legacy distinction
		/// SHARED/DUPLICATED Method in GenBOE Web => Common => SiteMasterUtilites.cs
		/// </summary>
		/// <param name="resourceData">Original Resources list</param>
		/// <param name="isBrc">Bool to signify if Resources are of type Business Resource Codes</param>
		/// <returns>Filtered list of Resources</returns>
		public static ICollection<ResourceDTO> GetResourcesBasedOnCompanyMode(ICollection<ResourceDTO> resourceData, bool isBrc)
		{
			if (Utilities.IsBRCEnabledForSystem)
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
				{
					if (!isBrc)
					{
						resourceData = resourceData.Where(x => x.SegRegion != WebConstants.SPACE_1LMX_CORE && x.SegRegion != WebConstants.SPACE_1LMX_SERVICES).ToList();
					}
					else
					{
						resourceData = resourceData.Where(x => x.SegRegion == WebConstants.SPACE_1LMX_CORE || x.SegRegion == WebConstants.SPACE_1LMX_SERVICES).ToList();
					}
				}

				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
				{
					if (!isBrc)
					{
						resourceData = resourceData.Where(x => x.SegRegion != WebConstants.RMS_1LMX_CORE && x.SegRegion != WebConstants.RMS_1LMX_SERVICES).ToList();

					}
					else
					{
						resourceData = resourceData.Where(x => x.SegRegion == WebConstants.RMS_1LMX_CORE || x.SegRegion == WebConstants.RMS_1LMX_SERVICES).ToList();
					}
				}
			}

			return resourceData;
		}

		/// <summary>
		/// Process the labor types for BRC in order to display the proper details in exports
		/// </summary>
		/// <param name="taskElementLabors">the task element labors</param>
		/// <returns>The labor types properly processed for resource vs BRC</returns>
		public static ICollection<ResourceTypeDto> ProcessLaborTypesForBrc(ICollection<ResourceTypeDto> taskElementLabors)
		{
			_ = taskElementLabors ?? throw new ArgumentNullException(nameof(taskElementLabors));

			ICollection<ResourceTypeDto> laborTypes = new Collection<ResourceTypeDto>();

			foreach (ResourceTypeDto laborType in taskElementLabors)
			{
				if (laborType.StartDate < Utilities.OneLmxStartDate && laborType.EndDate >= Utilities.OneLmxStartDate && laborType.BusinessResourceCodeID != null)
				{
					// Resource and BRC - split labor type into 2, one for Resource and one for BRC
					ResourceTypeDto resourceLaborType = laborType.DeepClone();
					resourceLaborType.LaborSpreads = resourceLaborType.LaborSpreads.Where(x => x.LaborSpreadDate < Utilities.OneLmxStartDate).ToCollection();
					resourceLaborType.ValueSpread = resourceLaborType.LaborSpreads.Sum(x => x.LaborSpreadValue);
					resourceLaborType.EndDate = resourceLaborType.LaborSpreads.Last().LaborSpreadDate;
					laborTypes.Add(resourceLaborType);

					ResourceTypeDto brcLaborType = laborType.DeepClone();
					brcLaborType.ResourceID = brcLaborType.BusinessResourceCodeID;
					brcLaborType.LaborSpreads = brcLaborType.LaborSpreads.Where(x => x.LaborSpreadDate >= Utilities.OneLmxStartDate).ToCollection();
					brcLaborType.ValueSpread = brcLaborType.LaborSpreads.Sum(x => x.LaborSpreadValue);
					brcLaborType.StartDate = brcLaborType.LaborSpreads.First().LaborSpreadDate;
					laborTypes.Add(brcLaborType);
				}
				else if (laborType.StartDate >= Utilities.OneLmxStartDate && laborType.BusinessResourceCodeID != null)
				{
					// BRC Only - Change the resource ID to the BRC ID before adding
					ResourceTypeDto brcLaborType = laborType.DeepClone();
					brcLaborType.ResourceID = brcLaborType.BusinessResourceCodeID;
					laborTypes.Add(brcLaborType);
				}
				else
				{
					// Resource Only - add the labor type as usual
					laborTypes.Add(laborType);
				}
			}

			return laborTypes;
		}
	}
}
