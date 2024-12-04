namespace GenBOE.ActionLogic.ModelView
{
	/// <summary>
	/// Totals row information for Skill Mix Tables
	/// </summary>
	public class SkillMixTotalsModelView
	{
		/// <summary>
		/// Historical Hours totals
		/// </summary>
		public decimal HistoricalHours { get; set; }

		/// <summary>
		/// Labor skill Mix totals
		/// </summary>
		public decimal LaborSkillMix { get; set; }

		/// <summary>
		/// BOE Skill Mix Totals
		/// </summary>
		public decimal BoeSkillMix { get; set; }

		/// <summary>
		/// Proposed Hours Totals
		/// </summary>
		public decimal ProposedHours { get; set; }
	}
}
