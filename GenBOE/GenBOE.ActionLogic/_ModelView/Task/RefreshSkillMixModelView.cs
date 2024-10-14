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
		/// Skill Mix Totals row
		/// </summary>
		public SkillMixTotalsModelView SkillMixTotals { get; set; } = new SkillMixTotalsModelView();

		/// <summary>
		/// Common Disclosure Totals row
		/// </summary>
		public SkillMixTotalsModelView CommonDisclosureTotals { get; set; } = new SkillMixTotalsModelView();
	}
}
