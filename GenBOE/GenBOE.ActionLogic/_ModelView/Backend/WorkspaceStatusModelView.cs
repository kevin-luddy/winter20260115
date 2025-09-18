// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView.Backend
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.Dtos;
	using IES.Common;
	using System;

	/// <summary>
	/// Workspace Status Model View
	/// </summary>
	public class WorkspaceStatusModelView : PersistedDataModelView
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public WorkspaceStatusModelView()
			: base()
		{
		}

		/// <summary>
		/// Constructor that takes in Workspace DTO to determine status
		/// </summary>
		/// <param name="inWorkspaceDTO">Workspace DTO</param>
		/// <exception cref="ArgumentNullException">Null Reference Exception</exception>
		public WorkspaceStatusModelView(WorkspaceDTO inWorkspaceDTO)
			: base()
		{
			if (inWorkspaceDTO == null)
			{
				throw new ArgumentNullException(nameof(inWorkspaceDTO));
			}

			WorkspaceStatus = inWorkspaceDTO.WorkspaceState;
			UpdateDate = inWorkspaceDTO.UpdateDate;
			WorkspaceName = inWorkspaceDTO.WorkspaceName;
			IsProjectMapWs = inWorkspaceDTO.IsProjectMapWorkspace;
		}

		/// <summary>
		/// Workspace Status Enum
		/// </summary>
		public WorkspaceState WorkspaceStatus { get; set; }

		/// <summary>
		/// Workspace Name
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Is Project Map Workspace
		/// </summary>
		public bool IsProjectMapWs { get; set; }
	}
}
