// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System;
	using GenBOE.Dtos;

	[Serializable]
	public class SkillMixModelView : SkillMixDTO
    {
		/// <summary>
		/// Ctor
		/// </summary>
		public SkillMixModelView()
		{
		}

		public SkillMixModelView(SkillMixDTO skillMixDTO)
        {
			if (skillMixDTO != null)
			{
				this.Included = skillMixDTO.Included;
				this.SkillMixID = skillMixDTO.SkillMixID;
				this.Rationale = skillMixDTO.Rationale;
				this.ProposedHours = skillMixDTO.ProposedHours;
				this.HistoricalHours = skillMixDTO.HistoricalHours;
				this.BOESkillMix = skillMixDTO.BOESkillMix;
				this.LaborSkillMix = skillMixDTO.LaborSkillMix;
				this.ResourceOld = skillMixDTO.ResourceOld;
				this.ResourceNew = skillMixDTO.ResourceNew;
				this.BOEID = skillMixDTO.BOEID;
				this.BOETaskElementID = skillMixDTO.BOETaskElementID;
				this.MOQTypeSelectionID = skillMixDTO.MOQTypeSelectionID;
				this.IsPercentLocked = skillMixDTO.IsPercentLocked;
			}
        }

		/// <summary>
		/// Converts the modelview into a DTO
		/// </summary>
		/// <returns>DTO version of this modelview</returns>
		public SkillMixDTO ToDto()
		{
			return new SkillMixDTO
			{
				Included = this.Included,
				SkillMixID = this.SkillMixID,
				Rationale = this.Rationale ?? string.Empty, 
				ProposedHours = this.ProposedHours,
				HistoricalHours = this.HistoricalHours,
				BOESkillMix = this.BOESkillMix,
				LaborSkillMix = this.LaborSkillMix,
				ResourceNew = this.ResourceNew ?? string.Empty,
				ResourceOld = this.ResourceOld ?? string.Empty,
				BOEID = this.BOEID,
				BOETaskElementID = this.BOETaskElementID,
				MOQTypeSelectionID = this.MOQTypeSelectionID,
				IsPercentLocked = this.IsPercentLocked
			};
		}
    }
}