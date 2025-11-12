// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.ModelView
{
	using System;
	using GenBOE.DataBridge.Core.DTO;

	/// <summary>
	/// Model view associated with the skill mix summary DTO
	/// </summary>
	[Serializable]
	public class SkillMixSummaryModelView : SkillMixSummaryDTO
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SkillMixSummaryModelView()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="summaryDTO">Skill Mix Summary DTO used to populate modelview</param>
		public SkillMixSummaryModelView(SkillMixSummaryDTO summaryDTO)
		{
			if (summaryDTO != null)
			{
				this.Included = summaryDTO.Included;
				this.Rationale = summaryDTO.Rationale;
				this.ProposedLegacyResource = summaryDTO.ProposedLegacyResource;
				this.HistoricalHours = summaryDTO.HistoricalHours;
				this.ResourceHours = summaryDTO.ResourceHours;
				this.ProposedBrc = summaryDTO.ProposedBrc;
				this.ProposedSkillMix = summaryDTO.ProposedSkillMix;
				this.HistoricalSkillMix = summaryDTO.HistoricalSkillMix;
				this.SkillMixSummaryID = summaryDTO.SkillMixSummaryID;
				this.BOEID = summaryDTO.BOEID;
				this.BOETaskElementID = summaryDTO.BOETaskElementID;
				this.BusinessResourceID = summaryDTO.BusinessResourceID;
				this.ResourceID = summaryDTO.ResourceID;
				this.IsUserInput = summaryDTO.IsUserInput;
			}
		}

		/// <summary>
		/// Converts the modelview into a DTO
		/// </summary>
		/// <returns>DTO version of this modelview</returns>
		public SkillMixSummaryDTO ToDto()
		{
			return new SkillMixSummaryDTO
			{
				Included = this.Included,
				Rationale = this.Rationale ?? string.Empty,
				ProposedLegacyResource = this.ProposedLegacyResource,
				HistoricalHours = this.HistoricalHours,
				ResourceHours = this.ResourceHours,
				ProposedBrc = this.ProposedBrc,
				ProposedSkillMix = this.ProposedSkillMix,
				HistoricalSkillMix = this.HistoricalSkillMix,
				ResourceID = this.ResourceID ?? string.Empty,
				BusinessResourceID = this.BusinessResourceID ?? string.Empty,
				SkillMixSummaryID = this.SkillMixSummaryID,
				BOEID = this.BOEID,
				BOETaskElementID = this.BOETaskElementID,
				IsUserInput = this.IsUserInput,
			};
		}

		/// <summary>
		/// UCOT Hours, Space only
		/// </summary>
		public decimal UCOTHours { get; set; }

		/// <summary>
		/// Grand Total Hours (proposed + UCOT Hours), Space only
		/// </summary>
		public decimal GrandTotalHours => this.TotalProposedLegacyBrc + this.UCOTHours;

		/// <summary>
		/// Total Proposed Hours (Legacy + BRC Hours), Space only
		/// </summary>
		public decimal TotalProposedLegacyBrc => this.ProposedLegacyResource + this.ProposedBrc;
	}
}
