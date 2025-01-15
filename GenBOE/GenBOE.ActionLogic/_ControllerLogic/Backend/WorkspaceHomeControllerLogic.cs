// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using System;

	/// <summary>
	/// Workspace home controller logic class (service).
	/// </summary>
	public class WorkspaceHomeControllerLogic
	{
		/// <summary>
		/// Common data mapper.
		/// </summary>
		private ICommonDataMapper commonDataMapper { get; set; }

		/// <summary>
		/// ctor.
		/// </summary>
		public WorkspaceHomeControllerLogic(ICommonDataMapper commonDataMapper)
		{
			this.commonDataMapper = commonDataMapper;
		}

		/// <summary>
		/// Get workspace master view data (proposal name, status, etc.)
		/// </summary>
		/// <returns>Master view data for the top of the page.</returns>
		public WorkspaceMasterViewModel GetWorkspaceMasterViewData(WorkspaceDTO ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			WorkspaceMasterViewModel masterViewData = new WorkspaceMasterViewModel();

			if (ws.Id == 0)
			{
				// if the workspace doesn't exist .. for example if we are running a system job
				masterViewData.ProposalName = "No Workspace";

				masterViewData.WorkspaceState = commonDataMapper.getWorkspaceStateName(WorkspaceState.None);

				masterViewData.Header = "Lockheed Martin Proprietary Information";
			}
			else
			{
				masterViewData.ProposalName = ws.WorkspaceName;

				masterViewData.WorkspaceState = commonDataMapper.getWorkspaceStateName(ws.WorkspaceState);

				masterViewData.Header = ws.ContainsOCI ?
					"Organizational Conflict of Interest - Lockheed Martin Proprietary Information" :
					"Lockheed Martin Proprietary Information";
			}

			return masterViewData;
		}
	}
}