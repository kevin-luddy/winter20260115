// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using GenBOE.Dtos;

	/// <summary>
	/// Model view associated with the skill mix summary DTO
	/// </summary>
	[Serializable]
	public class SkillMixSummaryModelView : SkillMixSummaryDTO
	{
		public SkillMixSummaryModelView()
		{
		}

		public SkillMixSummaryModelView(SkillMixSummaryDTO disclosureDTO)
		{
			if (disclosureDTO != null)
			{
				this.Included = disclosureDTO.Included;
				this.Rationale = disclosureDTO.Rationale;
				this.ProposedHours = disclosureDTO.ProposedHours;
				this.HistoricalHours = disclosureDTO.HistoricalHours;
				this.ResourceHours = disclosureDTO.ResourceHours;
				this.BusinessResourceHours = disclosureDTO.BusinessResourceHours;
				this.BOESkillMix = disclosureDTO.BOESkillMix;
				this.LaborSkillMix = disclosureDTO.LaborSkillMix;
				this.SkillMixSummaryID = disclosureDTO.SkillMixSummaryID;
				this.BOEID = disclosureDTO.BOEID;
				this.BOETaskElementID = disclosureDTO.BOETaskElementID;
				this.BusinessResourceID = disclosureDTO.BusinessResourceID;
				this.ResourceID = disclosureDTO.ResourceID;
				this.IsUserInput = disclosureDTO.IsUserInput;
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
				ProposedHours = this.ProposedHours,
				HistoricalHours = this.HistoricalHours,
				ResourceHours = this.ResourceHours,
				BusinessResourceHours = this.BusinessResourceHours,
				BOESkillMix = this.BOESkillMix,
				LaborSkillMix = this.LaborSkillMix,
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
		public decimal GrandTotalHours => this.TotalProposedHours + this.UCOTHours;

		/// <summary>
		/// Total Proposed Hours (resource + BRC Hours), Space only
		/// </summary>
		public decimal TotalProposedHours => this.ResourceHours + this.BusinessResourceHours;
	}
}
