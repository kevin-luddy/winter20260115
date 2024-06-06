// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Used to retrieve skill mix table
    /// </summary>
    public class SkillMixTableQueryModelView
    {
        /// <summary>
		/// Resource Id
		/// </summary>
		public string ResourceId { get; set; }

		/// <summary>
		/// Gets or sets the Query Filters
		/// </summary>
		public string Filters { get; set; }
    }
}