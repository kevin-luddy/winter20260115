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

		/// <summary>
		/// BRC Proposed Hours Totals
		/// </summary>
		public decimal BRCProposedHours { get; set; }

		/// <summary>
		/// UCOT Hours Totals, Space only
		/// </summary>
		public decimal UCOTHours { get; set; }

		/// <summary>
		/// Grand Total Hours (Sum UCOT + total Proposed Hours), Space only
		/// </summary>
		public decimal GrandTotalHours => UCOTHours + TotalProposedHours;

		/// <summary>
		/// Total proposed hours (BRC + proposed hours)
		/// </summary>
		public decimal TotalProposedHours => BRCProposedHours + ProposedHours;
	}
}
