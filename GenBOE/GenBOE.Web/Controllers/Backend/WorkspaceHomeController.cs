// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Transactions;
	using System.Web.Http;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using Microsoft.VisualBasic.Logging;

	/// <summary>
	/// Workspace Home Controller for getting workspace home data.
	/// </summary>
	public class WorkspaceHomeController : BoeDataBaseAPIController
	{
		#region Properties & Ctor
		/// <summary>
		/// Common data mapper.
		/// </summary>
		private ICommonDataMapper commonDataMapper { get; set; }

		/// <summary>
		/// Site master utils.
		/// </summary>
		private SiteMasterUtilities siteMasterUtilities { get; set; }

		/// <summary>
		/// System metrics.
		/// </summary>
		private SystemMetrics systemMetrics { get; set; }

		/// <summary>
		/// Service for genBOE Controller.
		/// </summary>
		private IGenBOEControllerLogic genBOEControllerLogic { get; set; }

		/// <summary>
		/// Security Information for genBOE
		/// </summary>
		private ISecurityInformation securityInformation { get; set; }

		/// <summary>
		/// Service for WorkspaceHomeController
		/// </summary>
		private WorkspaceHomeControllerLogic workspaceHomeControllerLogic { get; set; }

		private IWorkspaceControllerLogic workspaceLogic;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceHomeController");

		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceHomeController(ISecurityAccess securityAccess, IFullObjectFactory factory, IWorkspaceControllerLogic workspaceLogic, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, ICommonDataMapper commonDataMapper, SiteMasterUtilities siteMasterUtilities, SystemMetrics systemMetrics, IGenBOEControllerLogic genBOEControllerLogic, ISecurityInformation securityInformation, WorkspaceHomeControllerLogic workspaceHomeControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.commonDataMapper = commonDataMapper;
			this.siteMasterUtilities = siteMasterUtilities;
			this.systemMetrics = systemMetrics;
			this.genBOEControllerLogic = genBOEControllerLogic;
			this.securityInformation = securityInformation;
			this.workspaceHomeControllerLogic = workspaceHomeControllerLogic;
			this.workspaceLogic = workspaceLogic;
		}
		#endregion

		/// <summary>
		/// Gets the site menu items based on the workspace short name.
		/// </summary>
		/// <param name="workspaceShortname">Shortspace Name</param>
		/// <returns>Workspace menu items.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<GenBOEMasterMenuItemModelView> GetWorkspaceMenuItems(string workspaceShortname)
		{
			IESResponse<GenBOEMasterMenuItemModelView> result = new IESResponse<GenBOEMasterMenuItemModelView>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
				SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

				// Only return data if the permissions is at a read level or above.
				if (permission >= SecurityAuthorization.Read)
				{
					// Uses the existing GenBOE Controller because there is post processing logic happening after the site menu is constructed. This eliminates the need to replicate the logic here and confines it to one spot.
					using (GenBOEController controller = new GenBOEController(SecurityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, Factory, UserLoader, PermissionsLoader, genBOEControllerLogic))
					{
						ICollection<GenBOEMasterMenuItemModelView> menuItems = new Collection<GenBOEMasterMenuItemModelView>();

						System.Web.Mvc.ViewResult viewResult = controller.DisplaySiteMasterMenu(workspaceShortname);

						if (viewResult != null)
						{
							menuItems = (ICollection<GenBOEMasterMenuItemModelView>)viewResult.Model;
						}
						else
						{
							throw new GenValidationException("Workspace menu could not be constructed.");
						}

						result.Data = menuItems;
						result.IsSuccessful = true;
					}
				}
				else
				{
					result.Messages.Add($"Insufficient permissions for returning the Workspace menu data.");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace menu data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Gets the Homepage menu with UserMetrics and list of workspaces associated to user
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<GenBOEHomepageModelView> GetHomePageWorkspace()
		{
			IESSingleResponse<GenBOEHomepageModelView> result = new IESSingleResponse<GenBOEHomepageModelView>();
			GenBOEHomepageModelView model = new GenBOEHomepageModelView();

			try
			{
				UserDTO currentUser = UserLoader.GetUserForActiveUser();
				bool userIsSubcontractor = securityInformation.IsSubcontractorUser(currentUser.NTID, currentUser.IsSubcontractor);
				model.isReadOnly = SiteMasterUtilities.IsReadOnly();

				if (!userIsSubcontractor)
				{
					model.isSysAdmin = CheckPermission(SecurityPage.Admin, null) != SecurityAuthorization.None;
					if (model.isSysAdmin)
					{
						model.isReadOnly = false;
					}
					model.canCreateWS = CheckPermission(SecurityPage.CreateWorkspacePermissions, null) == SecurityAuthorization.CreateReadUpdateDelete;

					model.workspaceGridRows = workspaceHomeControllerLogic.GetHomepageGrid(model.isSysAdmin, currentUser, UserLoader, PermissionsLoader);

					result.Data = model;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning Workspace Grid Data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Ability to update favorite workspace
		/// </summary>
		/// <param name="workspaceId"></param>
		/// <param name="isFavorite"></param>
		/// <returns></returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> UpdateFavorite([FromBody]FavoriteModelView favoriteModelView)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			try
			{
				if (favoriteModelView != null)
				{
					UserDTO currentUser = UserLoader.GetUserForActiveUser();
					workspaceHomeControllerLogic.UpdateFavorite(currentUser, favoriteModelView.WorkspaceId, favoriteModelView.IsFavorite);
					result.Data = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add("Error saving Favorites");
			}

			return result;
		}

		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> DeleteWorkspaces(GenBOEHomepageWorkspaceRowModelView[] toBeDeleted)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (toBeDeleted == null || !toBeDeleted.Any())
			{
				throw new ArgumentNullException(nameof(toBeDeleted));
			}
			UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				foreach (GenBOEHomepageWorkspaceRowModelView wsToDelete in toBeDeleted)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(wsToDelete.WorkspaceShortName);
					workspaceHomeControllerLogic.UpdateDeletedStatus(wsToDelete.WorkspaceId, currentUser, true, ws.UpdateDate);
					this.Factory.ClearWorkspaceCache(ws.Shortname);
				}
				scope.Complete();
				result.Data = true;
				result.IsSuccessful = true;
			}
			return result;
		}


		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> RestorePtmWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();
			try
			{
				if (toBeRestored == null)
				{
					throw new ArgumentNullException(nameof(toBeRestored));
				}

				FullWorkspace ws = this.Factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName);
				UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
				if (String.IsNullOrEmpty(toBeRestored.TrackingNumber))
				{
					result.Messages.Add($"A tracking number is required to restore this workspace.");
					return result;
				}

				// see if this is a valid PTM Tracking Number
				int proposalId = workspaceHomeControllerLogic.GetProposalIdByTrackingNumber(toBeRestored.TrackingNumber);
				if (proposalId > 0)
				{
					// found the proposal
					GenTRAC.DataBridge.DTO.ProposalDto proposal = workspaceHomeControllerLogic.GetProposalByID(proposalId);

					// Copy over the PTM Proposal values from the Tracking Number and from UI
					ws.CostVolumeLeadPricerUserID = this.UserLoader.GetIdsByNtid(new string[] { toBeRestored.CostVolumeLeadPricerNtId }).First();
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
					result.Messages.Add($"PTM Tracking Number is invalid. Please choose another from the dropdown list.");
					return result;
				}

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					workspaceHomeControllerLogic.SaveIdentificationAndExportFormat(currentUser.UserID, ws);
					// get the updateDT from DB
					WorkspaceDTO dto = workspaceHomeControllerLogic.GetWorkspaceByID(toBeRestored.WorkspaceId);
					workspaceHomeControllerLogic.UpdateDeletedStatus(toBeRestored.WorkspaceId, currentUser, false, dto.UpdateDate);
					this.Factory.ClearWorkspaceCache(ws.Shortname);
					scope.Complete();
					result.Data = true;
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> RestoreWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (toBeRestored == null)
			{
				throw new ArgumentNullException(nameof(toBeRestored));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName);
			UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				workspaceHomeControllerLogic.UpdateDeletedStatus(toBeRestored.WorkspaceId, currentUser, false, ws.UpdateDate);
				this.Factory.ClearWorkspaceCache(ws.Shortname);
				scope.Complete();
				result.Data = true;
				result.IsSuccessful = true;
			}

			return result;
		}
	}
}