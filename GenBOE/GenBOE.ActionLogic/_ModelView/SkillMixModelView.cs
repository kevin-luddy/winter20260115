// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using GenBOE.Dtos;

    public class SkillMixModelView : SkillMixDTO
    {
		public string ResourceInput { get; set; }

		public SkillMixModelView()
		{
			this.ResourceInput = string.Empty;
		}

        public SkillMixModelView(SkillMixDTO skillMixDTO)
        {
			if (skillMixDTO != null)
			{
				this.ResourceInput = skillMixDTO.ResourceNew;
				
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
			}
        }
    }
}