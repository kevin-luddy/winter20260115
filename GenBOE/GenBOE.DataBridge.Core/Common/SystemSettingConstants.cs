// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common
{
	public static class SystemSettingConstants
	{
		public static readonly string PROJECT_MAP_OFFLOAD_TEXT = "ProjectMapOffloadText";
		public static readonly string JUSTIFYING_PUBLICATION = "JustifyingPublication";
		public static readonly string SAP_CLIENT_SECRET = "SapClientSecret";

		#region Skill Mix Settings
		public static readonly string SKILL_MIX_BLACKLIST = "SkillMixBlacklist";
		#endregion

		/// <summary>
		/// The value for sorting by Work Breakdown Structure
		/// </summary>
		public static readonly int EXPORT_SORT_WBS = 1;

		/// <summary>
		/// The value for sorting by Contract Line Item Number
		/// </summary>
		public static readonly int EXPORT_SORT_CLIN = 2;
	}
}


