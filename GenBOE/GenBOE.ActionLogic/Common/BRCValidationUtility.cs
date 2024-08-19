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
	using System.Collections.ObjectModel;
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
		/// <param name="workspaceShortname">Workspace shortname</param>
		/// <returns>Filtered list of Resources</returns>
		public static ICollection<ResourceDTO> GetResourcesBasedOnCompanyMode(ICollection<ResourceDTO> resourceData, bool isBrc, string workspaceShortname)
		{
			if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
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
						resourceData = resourceData.Where(x => x.SegRegion != WebConstants.RMS_1LMX_CORE && x.SegRegion != WebConstants.RMS_1LMX_SERVICES && x.SegRegion != WebConstants.RMS_1LMX_I_AND_N).ToList();

					}
					else
					{
						resourceData = resourceData.Where(x => x.SegRegion == WebConstants.RMS_1LMX_CORE || x.SegRegion == WebConstants.RMS_1LMX_SERVICES || x.SegRegion == WebConstants.RMS_1LMX_I_AND_N).ToList();
					}
				}
			}

			return resourceData;
		}

		/// <summary>
		/// Validates the Resource and Business Resource Code Required message based on Labor Type Start and End Date
		/// </summary>
		/// <param name="labor">Validation BOE Labor Type Model</param>
		/// <param name="workspaceShortname">Workspace Shortname</param>
		/// <returns>Validation string that labels required fields if any are missing, else empty string</returns>
		public static string ValidateResourceAndBusinessResourceCodeRequired(ResourceTypeDto labor, string workspaceShortname)
		{
			string requiredMessage = string.Empty;

			if (!Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
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
					else if (labor.StartDateValue < oneLMXStartDate && labor.EndDateValue >= oneLMXStartDate)
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
					else if (labor.StartDateValue >= oneLMXStartDate && !labor.BusinessResourceCodeID.HasValue)
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
		/// <param name="workspaceShortname">Workspace short name</param>
		public static void PopulateResourceAndBusinessResourceCodeHeaders(ValidationBOELaborType boeLabor, ResourceTypeDto labor, ICollection<ResourceDTO> resourcesFromTask, ICollection<ResourceDTO> businessResourceCodesFromTask, string workspaceShortname)
		{
			ResourceDTO resource = null;
			ResourceDTO businessResourceCode = null;

			if (boeLabor != null)
			{
				if (!Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
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

		/// <summary>
		/// Process the labor types for BRC in order to display the proper details in exports
		/// </summary>
		/// <param name="taskElementLabors">the task element labors</param>
		/// <param name="workspaceShortname">Workspace short name</param>
		/// <param name="startingNewId">Starting new psuedo ID for Sub Reource Type</param>
		/// <returns>The labor types properly processed for resource vs BRC</returns>
		public static ICollection<ResourceTypeDto> ProcessLaborTypesForBrc(ICollection<ResourceTypeDto> taskElementLabors, string workspaceShortname, int startingNewId = -1)
		{
			if (!Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
			{
				return taskElementLabors;
			}

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
					// Assigns a fake subresource type ID for proper identification during parsing of the Existing Spread.
					brcLaborType.Id = startingNewId--;
					brcLaborType.LaborSpreads = brcLaborType.LaborSpreads.Where(x => x.LaborSpreadDate >= Utilities.OneLmxStartDate).ToCollection();
					brcLaborType.ValueSpread = brcLaborType.LaborSpreads.Sum(x => x.LaborSpreadValue);
					brcLaborType.StartDate = brcLaborType.LaborSpreads.First().LaborSpreadDate;
					laborTypes.Add(brcLaborType);
				}
				else if (laborType.StartDate >= Utilities.OneLmxStartDate && laborType.BusinessResourceCodeID != null)
				{
					// BRC Only - Change the resource ID to the BRC ID before adding
					ResourceTypeDto brcLaborType = laborType.DeepClone();
					// Assigns a fake subresource type ID for proper identification during parsing of the Existing Spread.
					brcLaborType.Id = startingNewId--;
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
