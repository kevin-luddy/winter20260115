// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using GenBOE.ActionLogic._ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	public class WorkspaceHomeControllerLogic
	{
		/// <summary>
		/// WorkspaceDTODataLoader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader { get; set; }

		/// <summary>
		/// User loader
		/// </summary>
		protected IUserDTODataLoader userLoader { get; set; }

		/// <summary>
		/// Proposal loader
		/// </summary>
		private readonly GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader;

		/// <summary>
		/// Full object factory
		/// </summary>
		protected IFullObjectFactory factory { get; set; }

		/// <summary>
		/// Workspace logic
		/// </summary>
		private readonly IWorkspaceControllerLogic workspaceLogic;

		/// <summary>
		/// BOE metric loader
		/// </summary>
		private readonly IGenBOEMetricsDataLoader boeMetricsLoader;

		/// <summary>
		/// ActiveDirectoryUtilities
		/// </summary>
		private ActiveDirectoryUtilities adUtils { get; set; }

		public WorkspaceHomeControllerLogic(IWorkspaceControllerLogic workspaceLogic, IWorkspaceDTODataLoader workspaceDTODataLoader, GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader, IUserDTODataLoader userLoader, IFullObjectFactory factory, ActiveDirectoryUtilities adUtils, IGenBOEMetricsDataLoader boeMetricsLoader)
		{
			this.workspaceLogic = workspaceLogic;
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.proposalLoader = proposalLoader;
			this.userLoader = userLoader;
			this.adUtils = adUtils;
			this.factory = factory;
			this.boeMetricsLoader = boeMetricsLoader;
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
		/// Delete workspaces
		/// </summary>
		/// <param name="toBeDeleted">workspaces to be deleted</param>
		/// <returns>bool to indicate if operation is successful</returns>
		public bool DeleteWorkspaces(GenBOEHomepageWorkspaceRowModelView[] toBeDeleted)
		{
			bool result = false;

			if (toBeDeleted == null)
			{
				throw new ArgumentNullException(nameof(toBeDeleted));
			}

			UserDTO currentUser = this.userLoader.GetUserForActiveUser();
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				foreach (GenBOEHomepageWorkspaceRowModelView wsToDelete in toBeDeleted)
				{
					FullWorkspace ws = this.factory.CreateFullWorkspace(wsToDelete.WorkspaceShortName);
					workspaceDTODataLoader.UpdateDeletedStatus(wsToDelete.WorkspaceId, ws.UpdateDate, true, currentUser.UserID);
					this.factory.ClearWorkspaceCache(ws.Shortname);
				}
				scope.Complete();
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Restore workspace when not PTM Integrated
		/// </summary>
		/// <param name="toBeRestored">workspace to be restored</param>
		/// <returns>bool to indicate if operation is successful</returns>
		public bool RestoreWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			bool result = false;

			if (toBeRestored == null)
			{
				throw new ArgumentNullException(nameof(toBeRestored));
			}

			FullWorkspace ws = this.factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName);
			UserDTO currentUser = this.userLoader.GetUserForActiveUser();

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				workspaceDTODataLoader.UpdateDeletedStatus(toBeRestored.WorkspaceId, ws.UpdateDate, false, currentUser.UserID);
				this.factory.ClearWorkspaceCache(ws.Shortname);
				scope.Complete();
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Restore workspace when PTM Integrated
		/// </summary>
		/// <param name="toBeRestored">workspace to be restored</param>
		/// <returns>bool to indicate if operation is successful</returns>
		public bool RestorePtmWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			bool result = false;

			if (toBeRestored == null)
			{
				throw new ArgumentNullException(nameof(toBeRestored));
			}

			UserDTO currentUser = this.userLoader.GetUserForActiveUser();
			FullWorkspace ws = this.factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName);

			// Make sure a tracking number was selected or entered
			if (String.IsNullOrEmpty(toBeRestored.TrackingNumber))
			{
				throw new GenValidationException("A tracking number is required to restore this workspace.");
			}

			// see if this is a valid PTM Tracking Number
			int proposalId = proposalLoader.GetIdByTrackingNumber(toBeRestored.TrackingNumber);
			if (proposalId > 0)
			{
				// found the proposal
				GenTRAC.DataBridge.DTO.ProposalDto proposal = proposalLoader.GetById(proposalId);

				// Copy over the PTM Proposal values from the Tracking Number and from UI
				ws.CostVolumeLeadPricerUserID = userLoader.GetIdsByNtid(new string[] { toBeRestored.CostVolumeLeadPricerNtId }).First();
				ws.TrackingNumber = toBeRestored.TrackingNumber;
				ws.RFPNumber = proposal.RFPNumber;
				ws.ProposalTitle = proposal.ProposalTitle;
				if (proposal.ContractTypeIds.Any())
				{
					List<int> contractTypeIds = new List<int>();
					foreach (int contractTypeId in proposal.ContractTypeIds)
					{
						int convertedContractTypeId = this.workspaceLogic.ConvertPTMContractTypeId(contractTypeId);
						if (convertedContractTypeId > 0)
						{
							contractTypeIds.Add(convertedContractTypeId);
						}
					}

					ws.SelectedContractTypes = contractTypeIds.ToArray();
				}

				ws.ProposalClass = new PickListDto { Id = this.workspaceLogic.ConvertPTMProposalClassId(proposal.ProposalClass) };
				ws.LineOfBusiness = new PickListDto { Id = this.workspaceLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID) };
				ws.ProposalSubmittalDate = proposal.DeliveryDate;
				ws.RevisedSubmittalDate = proposal.RevisedSubmittalDate;
			}
			else
			{
				throw new GenValidationException("PTM Tracking Number is invalid. Please choose another from the dropdown list.");
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				workspaceDTODataLoader.SaveIdentificationAndExportFormat(currentUser.UserID, ws);
				// get the updateDT from DB
				WorkspaceDTO dto = workspaceDTODataLoader.GetById(toBeRestored.WorkspaceId);
				workspaceDTODataLoader.UpdateDeletedStatus(toBeRestored.WorkspaceId, dto.UpdateDate, false, currentUser.UserID);
				this.factory.ClearWorkspaceCache(ws.Shortname);
				scope.Complete();
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Sets up the inital display of "Who's online?".
		/// </summary>
		/// <returns>results for display</returns>
		public WhosOnlineGridModelView DisplayWhosOnline()
		{
			GenBOEUsersOnlineDTO systemMetricInfo = boeMetricsLoader.GetOnlineUserDetails();
			WhosOnlineGridModelView viewModel = new WhosOnlineGridModelView(systemMetricInfo);
			return viewModel;
		}
	}
}
