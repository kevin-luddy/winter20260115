// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;

	public class WorkspaceHomeControllerLogic
	{
		/// <summary>
		/// WorkspaceDTODataLoader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader { get; set; }
		private GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader;

		/// <summary>
		/// ActiveDirectoryUtilities
		/// </summary>
		private ActiveDirectoryUtilities adUtils { get; set; }

		public WorkspaceHomeControllerLogic(IWorkspaceDTODataLoader workspaceDTODataLoader, GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader, ActiveDirectoryUtilities adUtils)
		{
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.proposalLoader = proposalLoader;
			this.adUtils = adUtils;
		}

		/// <summary>
		/// Pulls Workspaces for the HomeGrid
		/// </summary>
		/// <param name="isSystemAdmin"></param>
		/// <param name="currentUser"></param>
		/// <param name="userLoader"></param>
		/// <param name="permissionsLoader"></param>
		/// <returns></returns>
		public ICollection<GenBOEHomepageWorkspaceRowModelView> GetHomepageGrid(bool isSystemAdmin, UserDTO currentUser, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
		{
			if (currentUser == null) 
			{
				throw new ArgumentNullException(nameof(currentUser));
			}

			if (userLoader == null) 
			{
				throw new ArgumentNullException(nameof(userLoader));
			}

			if (permissionsLoader == null)
			{
				throw new ArgumentNullException(nameof(permissionsLoader));
			}

			ICollection<GenBOEHomepageWorkspaceRowModelView> toReturn;
			// if you are system admin -> see all
			// else see a WS only if you are WS admin
			if (isSystemAdmin)
			{
				toReturn = workspaceDTODataLoader.GetAllWsForHomepageGrid(currentUser.UserID);
			}
			else
			{
				// get AD groups that the user belongs to
				ICollection<GroupData> usersGroups = adUtils.GetGroupsForUser(currentUser.NTID);

				// figure out which of those groups exist in the DB -> get their ETI User Ids
				List<string> userData = new List<string>();
				foreach (GroupData aGroup in usersGroups)
				{
					if (!userData.Contains(aGroup.Ntid))
					{
						userData.Add(aGroup.Ntid);
					}
				}

				ICollection<int> userIds = userLoader.GetIdsByNtid(userData);
				userIds.Add(currentUser.UserID);

				ICollection<int> workspaceIdsToWhichUserHasAccess = permissionsLoader.GetWorkspaceIdsThatUsersHaveAccessTo(userIds);
				ICollection<int> workspaceIdsWhereTheUserIsAdmin = permissionsLoader.GetWorkspaceIdsWhereUserIsWorkspaceAdmin(userIds);

				toReturn = this.workspaceDTODataLoader.GetWsForHomepageGrid(workspaceIdsToWhichUserHasAccess, workspaceIdsWhereTheUserIsAdmin, currentUser.UserID);
			}
			return toReturn;
		}

		/// <summary>
		/// Update the favorite in the Home Grid
		/// </summary>
		/// <param name="currentUser">current user that's doing the favorite</param>
		/// <param name="workspaceId">the workspace being favorited</param>
		/// <param name="isFavorite">the favorite boolean value</param>
		public void UpdateFavorite(UserDTO currentUser, int workspaceId, bool isFavorite)
		{
			if (currentUser == null)
			{
				throw new ArgumentNullException(nameof(currentUser));
			}

			workspaceDTODataLoader.UpdateFavorite(workspaceId, currentUser.UserID, isFavorite);
		}

		/// <summary>
		/// Update the favorite in the Home Grid
		/// </summary>
		/// <param name="currentUser">current user</param>
		/// <param name="workspaceId">the workspace being favorited</param>
		/// <param name="inSoftDelete">soft delete workspace/param>
		/// <param name="isFavorite">the favorite boolean value</param>
		public void UpdateDeletedStatus(int workspaceId, UserDTO currentUser, bool inSoftDelete, DateTime updateDate)
		{
			if (currentUser == null)
			{
				throw new ArgumentNullException(nameof(currentUser));
			}

			workspaceDTODataLoader.UpdateDeletedStatus(workspaceId, updateDate, inSoftDelete, currentUser.UserID);
		}

		/// <summary>
		/// Save the data within Workspace Identification and the Output Format Template
		/// </summary>
		/// <param name="userID">Curent user ID</param>
		/// <param name="wsToSave">the workspace identification and output format to save</param>
		public void SaveIdentificationAndExportFormat(int userID, WorkspaceDTO wsToSave)
		{
			workspaceDTODataLoader.SaveIdentificationAndExportFormat(userID, wsToSave);
		}

		/// <summary>
		/// Gets wprkspace by ID
		/// </summary>
		/// <param name="workspaceId">workspace Id</param>
		public WorkspaceDTO GetWorkspaceByID(int workspaceId)
		{
			return workspaceDTODataLoader.GetById(workspaceId);
		}

		/// <summary>
		/// Gets proposal by ID
		/// </summary>
		/// <param name="proposalId">proposal Id</param>
		public GenTRAC.DataBridge.DTO.ProposalDto GetProposalByID(int proposalId)
		{
			return proposalLoader.GetById(proposalId);
		}

		/// <summary>
		/// Gets proposal by tracking number
		/// </summary>
		/// <param name="trackingNumber">tracking number</param>
		public int GetProposalIdByTrackingNumber(string trackingNumber)
		{
			return proposalLoader.GetIdByTrackingNumber(trackingNumber);
		}
	}
}
