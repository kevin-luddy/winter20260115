// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// NLF Workspace Data Class (data returned from BOE)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE IN NLF, DO NOT CHANGE
	/// </summary>
	public class NlfWorkspaceData
	{
		/// <summary>
		/// Workspace Id
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// Workspace URL (shortname)
		/// </summary>
		public string WorkspaceUrl { get; set; }

		/// <summary>
		/// Workspace/Proposal (long) Name
		/// </summary>
		public string WorkspaceName { get; set; }
	}
}