// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.ModelView
{
	using System;
	using GenBOE.DataBridge.Core.DTO;

	[Serializable]
	public class CommonDisclosureModelView : CommonDisclosureSkillMixDTO
	{
		public CommonDisclosureModelView()
		{
		}

		public CommonDisclosureModelView(CommonDisclosureSkillMixDTO disclosureDTO)
		{
			if (disclosureDTO != null)
			{
				Included = disclosureDTO.Included;
				Rationale = disclosureDTO.Rationale;
				ProposedHours = disclosureDTO.ProposedHours;
				HistoricalHours = disclosureDTO.HistoricalHours;
				BOESkillMix = disclosureDTO.BOESkillMix;
				LaborSkillMix = disclosureDTO.LaborSkillMix;
				CommonDisclosureSkillMixID = disclosureDTO.CommonDisclosureSkillMixID;
				BOEID = disclosureDTO.BOEID;
				BOETaskElementID = disclosureDTO.BOETaskElementID;
				BusinessResourceID = disclosureDTO.BusinessResourceID;
				ResourceID = disclosureDTO.ResourceID;
				IsUserInput = disclosureDTO.IsUserInput;
			}
		}

		/// <summary>
		/// Converts the modelview into a DTO
		/// </summary>
		/// <returns>DTO version of this modelview</returns>
		public CommonDisclosureSkillMixDTO ToDto()
		{
			return new CommonDisclosureSkillMixDTO
			{
				Included = Included,
				Rationale = Rationale ?? string.Empty,
				ProposedHours = ProposedHours,
				HistoricalHours = HistoricalHours,
				BOESkillMix = BOESkillMix,
				LaborSkillMix = LaborSkillMix,
				ResourceID = ResourceID ?? string.Empty,
				BusinessResourceID = BusinessResourceID ?? string.Empty,
				CommonDisclosureSkillMixID = CommonDisclosureSkillMixID,
				BOEID = BOEID,
				BOETaskElementID = BOETaskElementID,
				IsUserInput = IsUserInput
			};
		}
	}
}
