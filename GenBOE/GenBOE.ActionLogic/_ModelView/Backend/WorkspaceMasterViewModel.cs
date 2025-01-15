// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	/// <summary>
	/// Workspace master view data.
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class WorkspaceMasterViewModel
	{
		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceMasterViewModel()
		{
			ProposalName = "Unable to get Proposal name";
			WorkspaceState = "Unable to get Workspace Status";
			Header = "Lockheed Martin Proprietary Information";
		}

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
