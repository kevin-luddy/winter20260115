// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using Microsoft.Practices.ObjectBuilder2;
	using IES.Common.classes;

	/// <summary>
	/// Utility Class to hold Action Logic Methods
	/// </summary>
	public class ActionLogicUtility
	{
		/// <summary>
		/// Retrieves the Proposal Title and RFP Number from the workspace.
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <returns>A string in the form { Proposal Number } / { RFP }</returns>
		[SuppressMessage("Microsoft.Design", "CA1011: Consider passing base types as parameters")]
		public static string GetProposalTitleAndRfpNumber(FullWorkspace workspace)
		{
			if (workspace != null)
			{

				return string.IsNullOrWhiteSpace(workspace.RFPNumber) ? $"{workspace.ProposalTitle}" : $"{workspace.ProposalTitle} / {workspace.RFPNumber}";
			}

			return string.Empty;
		}

		/// <summary>
		/// Validate the Skill Mix Table for any errors
		/// </summary>
		/// <param name="skillMixModels">SkillMix Models</param>
		/// <param name="onButtonPress">True if this validation is being performed as part of the Validate BOE button</param>
		/// <returns>A collection of any validation errors/messages</returns>
		public static ICollection<string> ValidateSkillMixTable(ICollection<SkillMixModelView> skillMixModels, bool onButtonPress)
		{
			ICollection<string> errorMessages = new Collection<string>();
			bool isSpace = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems;

			IList<SkillMixModelView> skillMixRowsEmptyBoeMixWhenIncluded = skillMixModels.Where(x => x.Included && !x.BOESkillMix.HasValue).ToList();
			IList<SkillMixModelView> skillMixRowsExceedChars = skillMixModels.Where(x => !string.IsNullOrEmpty(x.Rationale) && x.Rationale.Length > 255).ToList();
			IList<SkillMixModelView> skillMixRowsResourceOldExceedChars = skillMixModels.Where(x => !string.IsNullOrEmpty(x.ResourceOld) && x.ResourceOld.Length > 20).ToList();

			bool doesEmptyNullCurrentResourceExist = skillMixModels.Any(x => string.IsNullOrEmpty(x.ResourceNew) && x.Included);
			decimal totalSKillMixRowsBOESkillMix = skillMixModels.Where(p => p.BOESkillMix.HasValue).Sum(p => p.BOESkillMix.Value);

			// Applies the proper name for the Skill Mix table and the BOE Skill Mix column based on the company configuration mode.
			string skillMixTableName = isSpace ? Constants.SPACE_SKILL_MIX_TABLE_HEADER : Constants.RMS_SKILL_MIX_TABLE_HEADER;
			string BoeSkillMixColumnName = isSpace ? Constants.SPACE_BOE_SKILL_MIX_COLUMN_NAME : Constants.RMS_BOE_SKILL_MIX_COLUMN_NAME;

			// This check applies to both Space and RMS
			if (doesEmptyNullCurrentResourceExist)
			{
				errorMessages.Add($"{skillMixTableName}: Included cannot be set to 'Yes' for an empty/null Current Resource.");
			}

			foreach (string skillMixResourceOld in skillMixRowsEmptyBoeMixWhenIncluded.Select(x => x.ResourceOld))
			{
				errorMessages.Add($"{skillMixTableName}: {BoeSkillMixColumnName} is missing for {skillMixResourceOld}.");
			}

			if (!totalSKillMixRowsBOESkillMix.EqualsEpsilon(100) && !totalSKillMixRowsBOESkillMix.EqualsEpsilon(0))
			{
				errorMessages.Add($"{skillMixTableName}: {BoeSkillMixColumnName} total must be either 0% or 100%");
			}

			foreach (string skillMixResourceOld in skillMixRowsExceedChars.Select(x => x.ResourceOld))
			{
				errorMessages.Add($"{skillMixTableName}: The maximum length of the Rationale field for {skillMixResourceOld} is 255 characters.");
			}

			foreach (string skillMixResourceOld in skillMixRowsResourceOldExceedChars.Select(x => x.ResourceOld))
			{
				errorMessages.Add($"{skillMixTableName}: The maximum length of the Historical Resource field for {skillMixResourceOld} is 20 characters.");
			}

			if (onButtonPress)
			{
				IList<SkillMixModelView> skillMixRowsMissingRationale = skillMixModels.Where(x => string.IsNullOrEmpty(x.Rationale)).ToList();
				foreach (string skillMixResourceOld in skillMixRowsMissingRationale.Select(x => x.ResourceOld))
				{
					errorMessages.Add($"{skillMixTableName}: The Rationale field for {skillMixResourceOld} is required.");
				}
			}

			return errorMessages;
		}

		/// <summary>
		/// Validate the Common Disclosure Skill Mix Table for any errors
		/// </summary>
		/// <param name="commonDisclosures">Common Disclosures</param>
		/// <param name="onButtonPress">True if this validation is being performed as part of the Validate BOE button</param>
		/// <param name="hasResourceTypes">Does the Task contain resource types?</param>
		/// <returns>A collection of validation errors/messages</returns>
		public static ICollection<string> ValidateCommonDisclosureSkillMixTable(ICollection<CommonDisclosureModelView> commonDisclosures, bool onButtonPress, bool hasResourceTypes)
		{
			ICollection<string> errorMessages = new Collection<string>();

			IList<CommonDisclosureModelView> commonDisclosureRowsExceedChars = commonDisclosures
																			.Where(x => !string.IsNullOrEmpty(x.Rationale) && x.Rationale.Length > 255).ToList();
			IList<CommonDisclosureModelView> commonDisclosureRowsEmptyBoeSkillMixWhenIncluded = commonDisclosures
																			.Where(x => x.Included && !x.BOESkillMix.HasValue).ToList();
			IList<CommonDisclosureModelView> commonDisclosureRowsInvalidBoeSkillMixWhenIncluded = commonDisclosures
																			.Where(x => x.Included && x.BOESkillMix.HasValue && x.BOESkillMix.Value <= 0).ToList();
			IList<CommonDisclosureModelView> commonDisclosureIncludedHasTrueValue = commonDisclosures.Where(x => x.Included).ToList();
			decimal totalCommonDisclosureRowsBOESkillMix = commonDisclosures.Where(p => p.BOESkillMix.HasValue).Sum(p => p.BOESkillMix.Value);

			foreach (string skillMixResourceID in commonDisclosureRowsExceedChars.Select(x => x.ResourceID))
			{
				errorMessages.Add(string.Format("LM Enterprise Skill Mix Table: The maximum length of the Rationale field for {0} is {1} characters.", skillMixResourceID, 255));
			}

			foreach (string skillMixResourceID in commonDisclosureRowsEmptyBoeSkillMixWhenIncluded.Select(x => x.ResourceID))
			{
				errorMessages.Add(string.Format("LM Enterprise Skill Mix Table: BOE Skill Mix is missing for {0}.", skillMixResourceID));
			}

			foreach (string skillMixResourceID in commonDisclosureRowsInvalidBoeSkillMixWhenIncluded.Select(x => x.ResourceID))
			{
				errorMessages.Add(string.Format("LM Enterprise Skill Mix Table: BOE skill Mix has invalid value for {0}.", skillMixResourceID));
			}

			// A task is valid for save if it has no Resource Types and a Resource Type is needed to be marked as Included
			// So only perform this validation if there are Resource Types
			if (hasResourceTypes && commonDisclosureIncludedHasTrueValue.Count <= 0)
			{
				errorMessages.Add("LM Enterprise Skill Mix Table: At least one Resource has to be included");
			}

			if (!totalCommonDisclosureRowsBOESkillMix.EqualsEpsilon(100) && !totalCommonDisclosureRowsBOESkillMix.EqualsEpsilon(0))
			{
				errorMessages.Add("LM Enterprise Skill Mix Table: BOE Skill Mix total must be either 0% or 100%");
			}

			ValidateResourceAndBRCCombos(commonDisclosures, errorMessages);

			if (onButtonPress)
			{
				IList<CommonDisclosureModelView> commonDisclosureRowsMissingRationale = commonDisclosures.Where(x => string.IsNullOrEmpty(x.Rationale)).ToList();
				foreach (string skillMixResourceID in commonDisclosureRowsMissingRationale.Select(x => x.ResourceID))
				{
					errorMessages.Add($"LM Enterprise Skill Mix Table: The Rationale field for {skillMixResourceID} is required.");
				}
			}

			return errorMessages;
		}

		#region Private Methods

		/// <summary>
		/// Validate  each Resource and BRC combo is unique in the Common Disclosure Table
		/// </summary>
		/// <param name="commonDisclosures">Collection of Common Disclosure Data</param>
		/// <param name="errorMessages">Error Messages</param>
		private static void ValidateResourceAndBRCCombos(ICollection<CommonDisclosureModelView> commonDisclosures, ICollection<String> errorMessages)
		{
			// Create Dictionary of list for Dropdown
			Dictionary<string, List<CommonDisclosureModelView>> resourceToCDRowMap = commonDisclosures?.Where(cd => cd.ResourceID != null)
				?.GroupBy(cd => cd.ResourceID)
				?.ToDictionary(g => g.Key, g => g.ToList());

			resourceToCDRowMap.ForEach(pair =>
			{
				//don't need to count the null ones - that validation is checked above
				List<CommonDisclosureModelView> brcsForResource = pair.Value.Where(x => !string.IsNullOrEmpty(x.BusinessResourceID)).ToList();
				int brcsForResourceCount = brcsForResource.Count();
				bool isUnique = brcsForResource.Select(x => x.BusinessResourceID).Distinct().ToList().Count() == brcsForResourceCount;
				if (!isUnique)
				{
					errorMessages.Add(string.Format("LM Enterprise Skill Mix Table: Each BRC must be unique for Resource {0}.", pair.Key));
				}
			});
		}
		#endregion Private Methods
	}
}
