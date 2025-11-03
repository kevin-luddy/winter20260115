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
		/// Historical Labor skill Mix totals
		/// </summary>
		public decimal HistoricalSkillMix { get; set; }

		/// <summary>
		/// Proposed BOE Skill Mix Totals
		/// </summary>
		public decimal ProposedSkillMix { get; set; }

		/// <summary>
		/// Proposed Legacy Hours Totals
		/// </summary>
		public decimal ProposedLegacyResource { get; set; }

		/// <summary>
		/// BRC Proposed Hours Totals
		/// </summary>
		public decimal ProposedBrc { get; set; }

		/// <summary>
		/// UCOT Hours Totals, Space only
		/// </summary>
		public decimal UCOTHours { get; set; }

		/// <summary>
		/// Grand Total Hours (Sum UCOT + total Proposed Hours), Space only
		/// </summary>
		public decimal GrandTotalHours => UCOTHours + TotalProposedLegacyBrc;

		/// <summary>
		/// Total proposed hours (BRC + resource hours)
		/// </summary>
		public decimal TotalProposedLegacyBrc => ProposedBrc + ProposedLegacyResource;
	}
}
