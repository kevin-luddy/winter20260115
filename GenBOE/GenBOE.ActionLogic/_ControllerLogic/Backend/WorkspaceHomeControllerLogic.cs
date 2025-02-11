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

		/// <summary>
		/// ActiveDirectoryUtilities
		/// </summary>
		private ActiveDirectoryUtilities adUtils { get; set; }

		public WorkspaceHomeControllerLogic(IWorkspaceDTODataLoader workspaceDTODataLoader, ActiveDirectoryUtilities adUtils)
		{
			this.workspaceDTODataLoader = workspaceDTODataLoader;
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
	}
}
