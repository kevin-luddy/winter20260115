// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.PLDStatusSync
{
	using System;
	using System.Collections.Generic;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.Enums;

	public class Program
	{
		private static IWorkspaceDTODataLoader workspaceLoader;
		private static IPldDTODataLoader pldLoader;
		private static readonly Logger logger = new Logger(typeof(Program));

		public static void Main(string[] args)
		{
			if (Utilities.ShowPLDIsIntegrated)
			{
				logger.Info("PLD Status Sync: Beginning sync");
				int updatedWorkspaces = 0;

				try
				{
					// Initialized loaders
					workspaceLoader = new WorkspaceDTODataLoader();
					LineOfBusinessDataLoader lobLoader = new LineOfBusinessDataLoader();
					pldLoader = new PldDTODataLoader(lobLoader);

					string awardedFinalized = PLDStatus.AwardedFinalized.GetDescription();
					string canceled = PLDStatus.Canceled.GetDescription();

					// Get all workspaces with PA number
					ICollection<WorkspaceDTO> workspaces = workspaceLoader.GetAllWorkspacesWithTrackingNumbers();
					foreach (WorkspaceDTO workspace in workspaces)
					{
						try
						{
							// Get PA for the number
							PLDProposalDTO pldPA = pldLoader.GetProposalDetails(workspace.TrackingNumber);
							if (pldPA != null)
							{
								// If PA exists and has Awarded/Finalized Status, move the Workspace to Complete if not already in that state
								// If PA is Canceled, move the Workspace to Closed if not already in that state
								if (pldPA.ProposalStatus.Trim() == awardedFinalized && workspace.WorkspaceState != WorkspaceState.Complete)
								{
									workspace.WorkspaceState = WorkspaceState.Complete;
									workspaceLoader.SaveWorkspaceSettings(workspace.CreatedByUserID, workspace);
									logger.Info($"PLD Status Sync: Updated Workspace State for {workspace.WorkspaceName} from {workspace.WorkspaceState.GetDescription()} to Complete");
									updatedWorkspaces++;
								}
								else if (pldPA.ProposalStatus.Trim() == canceled && workspace.WorkspaceState != WorkspaceState.Closed)
								{
									workspace.WorkspaceState = WorkspaceState.Closed;
									workspaceLoader.SaveWorkspaceSettings(workspace.CreatedByUserID, workspace);
									logger.Info($"PLD Status Sync: Updated Workspace State for {workspace.WorkspaceName} from {workspace.WorkspaceState.GetDescription()} to Closed");
									updatedWorkspaces++;
								}
							}
							else
							{
								logger.Error($"PLD Status Sync: PA {workspace.TrackingNumber} not found for workspace {workspace.WorkspaceName}");
							}
						}
						catch (Exception ex)
						{
							logger.Error($"PLD Status Sync: Error syncing workspace {workspace.WorkspaceName}: {ex}");
						}
					}
				}
				catch (Exception ex)
				{
					logger.Error($"PLD Status Sync: Error duing sync: {ex}");
				}

				logger.Info($"PLD Status Sync: Sync complete - {updatedWorkspaces} Workspace(s) updated");
			}
		}
	}
}
