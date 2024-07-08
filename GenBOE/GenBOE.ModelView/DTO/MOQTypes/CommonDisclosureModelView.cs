// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using GenBOE.Dtos;

	public class CommonDisclosureModelView : CommonDisclosureSkillMixDTO
	{
		public CommonDisclosureModelView()
		{
		}

		public CommonDisclosureModelView(CommonDisclosureSkillMixDTO disclosureDTO)
		{
			if (disclosureDTO != null)
			{
				this.Included = disclosureDTO.Included;
				this.SkillMixID = disclosureDTO.SkillMixID;
				this.Rationale = disclosureDTO.Rationale;
				this.ProposedHours = disclosureDTO.ProposedHours;
				this.HistoricalHours = disclosureDTO.HistoricalHours;
				this.BOESkillMix = disclosureDTO.BOESkillMix;
				this.LaborSkillMix = disclosureDTO.LaborSkillMix;
				this.SkillMixID = disclosureDTO.SkillMixID;
				this.CommonDisclosureSkillMixID = disclosureDTO.CommonDisclosureSkillMixID;
				this.BOEID = disclosureDTO.BOEID;
				this.BOETaskElementID = disclosureDTO.BOETaskElementID;
				this.MOQTypeSelectionID = disclosureDTO.MOQTypeSelectionID;
				this.BusinessResourceID = disclosureDTO.BusinessResourceID;
				this.ResourceID = disclosureDTO.ResourceID;
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
				Included = this.Included,
				SkillMixID = this.SkillMixID,
				Rationale = this.Rationale ?? string.Empty,
				ProposedHours = this.ProposedHours,
				HistoricalHours = this.HistoricalHours,
				BOESkillMix = this.BOESkillMix,
				LaborSkillMix = this.LaborSkillMix,
				ResourceID = this.ResourceID ?? string.Empty,
				BusinessResourceID = this.BusinessResourceID ?? string.Empty,
				CommonDisclosureSkillMixID = this.CommonDisclosureSkillMixID,
				BOEID = this.BOEID,
				BOETaskElementID = this.BOETaskElementID,
				MOQTypeSelectionID = this.MOQTypeSelectionID,
			};
		}
	}
}
