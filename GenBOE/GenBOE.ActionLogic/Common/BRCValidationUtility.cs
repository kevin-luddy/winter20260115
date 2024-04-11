// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Class utilized to validate Business Resource Code
	/// </summary>
	public static class BRCValidationUtility
	{
		/// <summary>
		/// Returns Resources / Business Resource Codes based on Company mode and 1LMX or Legacy distinction
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
		/// Validates the Resource and Business Resource Code Required message based on Labor Type Start and End Date
		/// </summary>
		/// <param name="labor">Validation BOE Labor Type Model</param>
		/// <returns>Validation string that labels required fields if any are missing, else empty string</returns>
		public static string ValidateResourceAndBusinessResourceCodeRequired(ResourceTypeDto labor)
		{
			string requiredMessage = string.Empty;

			if (!Utilities.IsBRCEnabledForSystem)
			{
				if (labor != null && !labor.ResourceID.HasValue)
				{
					requiredMessage = BoeDTO.RESOURCE_CODE_REQUIRED;
				}
			}
			else
			{
				DateTime oneLMXStartDate = Utilities.OneLmxStartDate;

				if (labor != null)
				{
					if (labor.EndDateValue < oneLMXStartDate && !labor.ResourceID.HasValue)
					{
						requiredMessage = BoeDTO.RESOURCE_CODE_REQUIRED;
					}

					if (labor.StartDateValue < oneLMXStartDate && labor.EndDateValue >= oneLMXStartDate)
					{
						if (!labor.ResourceID.HasValue)
						{
							requiredMessage = BoeDTO.RESOURCE_CODE_REQUIRED;
						}
						else if (!labor.ResourceID.HasValue && !labor.BusinessResourceCodeID.HasValue)
						{
							requiredMessage = BoeDTO.RESOURCE_AND_BUSINESS_RESOURCE_CODE_REQUIRED;
						}
						else if (!labor.BusinessResourceCodeID.HasValue)
						{
							requiredMessage = BoeDTO.BUSINESS_RESOURCE_CODE_REQUIRED;
						}
					}

					if (labor.StartDateValue >= oneLMXStartDate && !labor.BusinessResourceCodeID.HasValue)
					{
						requiredMessage = BoeDTO.BUSINESS_RESOURCE_CODE_REQUIRED;
					}
				}
			}

			return requiredMessage;
		}

		/// <summary>
		/// Populate Labor Type Header for BOE Labor
		/// </summary>
		/// <param name="boeLabor">Validation BOE Labor Type Model</param>
		/// <param name="labor">Labor</param>
		/// <param name="resourcesFromTask">Resources for Task</param>
		/// <param name="businessResourceCodesFromTask">Business Resource Codes for Task</param>
		public static void PopulateResourceAndBusinessResourceCodeHeaders(ValidationBOELaborType boeLabor, ResourceTypeDto labor, ICollection<ResourceDTO> resourcesFromTask, ICollection<ResourceDTO> businessResourceCodesFromTask)
		{
			ResourceDTO resource = null;
			ResourceDTO businessResourceCode = null;

			if (boeLabor != null)
			{
				if (!Utilities.IsBRCEnabledForSystem)
				{
					if (labor.ResourceID.HasValue)
					{
						resource = resourcesFromTask.FirstOrDefault(x => x.Id == labor.ResourceID.Value);
					}

					if (resource == null)
					{
						boeLabor.LaborTypeHeader = "Resource Type: (Not Selected)";
					}
					else
					{
						boeLabor.LaborTypeHeader = "Resource Type: " + (resource.ResourceName ?? string.Empty);
					}
				}
				else
				{
					if (labor.ResourceID.HasValue)
					{
						resource = resourcesFromTask.FirstOrDefault(x => x.Id == labor.ResourceID.Value);
					}

					if (labor.BusinessResourceCodeID.HasValue)
					{
						businessResourceCode = businessResourceCodesFromTask.FirstOrDefault(x => x.Id == labor.BusinessResourceCodeID.Value);
					}

					if (resource == null && businessResourceCode == null)
					{
						boeLabor.LaborTypeHeader = "Resource Type: (Not Selected); Business Resource Code Type: (Not Selected)";
					}
					else if (resource != null && businessResourceCode != null)
					{
						boeLabor.LaborTypeHeader = "Resource Type: " + (resource.ResourceName ?? string.Empty) + " Business Resource Code Type: " + (businessResourceCode.ResourceName ?? string.Empty);
					}
					else if (resource != null && businessResourceCode == null)
					{
						boeLabor.LaborTypeHeader = "Resource Type: " + (resource.ResourceName ?? string.Empty) + " Business Resource Code Type: (Not Selected)";
					}
					else if (resource == null && businessResourceCode != null)
					{
						boeLabor.LaborTypeHeader = "Resource Type: (Not Selected); Business Resource Code Type: " + (businessResourceCode.ResourceName ?? string.Empty);
					}
				}
			}
		}
	}
}
