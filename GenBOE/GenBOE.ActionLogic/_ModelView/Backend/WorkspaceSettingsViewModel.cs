// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	/// <summary>
	/// Workspace Settings Class
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class WorkspaceSettingsViewModel
	{
		/// <summary>
		/// Is BRC Enabled for the workspace?
		/// </summary>
		public bool IsBRCEnabled { get; set; }

		/// <summary>
		/// Is SAP Enabled for the workspace?
		/// </summary>
		public bool IsSAPEnabled { get; set; }

		/// <summary>
		/// Is the workspace before the SAP cutoff? (always false for RMS)
		/// </summary>
		public bool IsWorkspaceBeforeSAPCutoff { get; set; }

		/// <summary>
		/// Should SAP be shown for the workspace?
		/// </summary>
		public bool ShowSAPForWorkspace { get; set; }

		/// <summary>
		/// Should Skill Mix be shown for the Workspace?
		/// </summary>
		public bool ShowSkillMixForWorkspace { get; set; }

		/// <summary>
		/// Is historical reference explanation required for the workspace?
		/// </summary>
		public bool IsHistoricalReferenceExplanationRequired { get; set; }

		/// <summary>
		/// Proposal name.
		/// </summary>
		public string ProposalName { get; set; }

		/// <summary>
		/// Workspace status.
		/// </summary>
		public string WorkspaceState { get; set; }

		/// <summary>
		/// Dynamic header for LMPI (and/or OCI).
		/// </summary>
		public string Header { get; set; }
	}
}