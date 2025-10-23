namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;
	using GenBOE.DataBridge.DTO;

	public class RefreshSkillMixModelView
	{
		/// <summary>
		/// Skill Mix Rows
		/// </summary>
		public ICollection<SkillMixModelView> SkillMixRows { get; set; } = new List<SkillMixModelView>();

		/// <summary>
		/// Common Disclosure Rows
		/// </summary>
		public ICollection<CommonDisclosureModelView> CommonDisclosureRows { get; set; } = new List<CommonDisclosureModelView>();

		/// <summary>
		/// Skill Mix Summary Rows
		/// </summary>
		public ICollection<SkillMixSummaryModelView> SkillMixSummaryRows { get; set; } = new List<SkillMixSummaryModelView>();

		/// <summary>
		/// Skill Mix Totals row
		/// </summary>
		public SkillMixTotalsModelView SkillMixTotals { get; set; } = new SkillMixTotalsModelView();

		/// <summary>
		/// Common Disclosure Totals row
		/// </summary>
		public SkillMixTotalsModelView CommonDisclosureTotals { get; set; } = new SkillMixTotalsModelView();

		/// <summary>
		/// Skill Mix Summary Totals row
		/// </summary>
		public SkillMixTotalsModelView SkillMixSummaryTotals { get; set; } = new SkillMixTotalsModelView();
	}
}
