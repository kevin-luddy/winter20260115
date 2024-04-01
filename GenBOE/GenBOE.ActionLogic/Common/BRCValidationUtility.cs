// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Class utilized to validate Business Resource Code
	/// </summary>
	public static class BRCValidationUtility
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
	}
}
