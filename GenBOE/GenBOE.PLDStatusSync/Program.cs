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
	using IES.Common.classes;
	using IES.Common.Enums;

	public class Program
	{
		private static IWorkspaceDTODataLoader workspaceLoader;
		private static IPldDTODataLoader pldLoader;

		public static void Main(string[] args)
		{
			if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST && Utilities.ShowPLDIsIntegrated)
			{
				Console.WriteLine("Running PLD Status Sync for RMS");

				// Initialized loaders
				workspaceLoader = new WorkspaceDTODataLoader();
				LineOfBusinessDataLoader lobLoader = new LineOfBusinessDataLoader();
				pldLoader = new PldDTODataLoader(lobLoader);

				// Get all workspaces with PA number
				ICollection<WorkspaceDTO> workspaces = workspaceLoader.GetAllWorkspacesWithTrackingNumbers();
				foreach (WorkspaceDTO workspace in workspaces)
				{
					Console.WriteLine("-----------------------------");
					Console.WriteLine($"Syncing status for {workspace.WorkspaceName} with PA Number {workspace.TrackingNumber}");

					// Get PA for the number
					PLDProposalDTO pldPA = pldLoader.GetProposalDetails(workspace.TrackingNumber);
					if (pldPA != null)
					{
						// If PA exists and has Awarded/Finalized Status, move the Workspace to Complete if not already in that state
						// If PA is Canceled, move the Workspace to Closed if not already in that state
						Console.WriteLine($"PA found with status {pldPA.ProposalStatus}");
						if (pldPA.ProposalStatus.Trim() == PLDStatus.AwardedFinalized.GetDescription() && workspace.WorkspaceState != WorkspaceState.Complete)
						{
							Console.WriteLine($"Updating Workspace State from {workspace.WorkspaceState.GetDescription()} to Complete");
							workspace.WorkspaceState = WorkspaceState.Complete;
							workspaceLoader.SaveWorkspaceSettings(workspace.CreatedByUserID, workspace);
						}
						else if (pldPA.ProposalStatus.Trim() == PLDStatus.Canceled.GetDescription() && workspace.WorkspaceState != WorkspaceState.Closed)
						{
							Console.WriteLine($"Updating Workspace State from {workspace.WorkspaceState.GetDescription()} to Closed");
							workspace.WorkspaceState = WorkspaceState.Closed;
							workspaceLoader.SaveWorkspaceSettings(workspace.CreatedByUserID, workspace);
						}
						else
						{
							Console.WriteLine($"Current Workspace State of {workspace.WorkspaceState.GetDescription()} will not be updated.");
						}
					}
					else
					{
						Console.WriteLine("No PA found");
					}
				}
			}
		}
	}
}
