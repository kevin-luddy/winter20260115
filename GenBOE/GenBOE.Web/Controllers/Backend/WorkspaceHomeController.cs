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

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceHomeController");

		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceHomeController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, ICommonDataMapper commonDataMapper, SiteMasterUtilities siteMasterUtilities, SystemMetrics systemMetrics, IGenBOEControllerLogic genBOEControllerLogic, ISecurityInformation securityInformation, WorkspaceHomeControllerLogic workspaceHomeControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.commonDataMapper = commonDataMapper;
			this.siteMasterUtilities = siteMasterUtilities;
			this.systemMetrics = systemMetrics;
			this.genBOEControllerLogic = genBOEControllerLogic;
			this.securityInformation = securityInformation;
			this.workspaceHomeControllerLogic = workspaceHomeControllerLogic;
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
				}
				
				model.workspaceGridRows = workspaceHomeControllerLogic.GetHomepageGrid(model.isSysAdmin, currentUser, UserLoader, PermissionsLoader);
				result.Data = model;
				
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace Grid Data: {ex.Message}");
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

		/// <summary>
		/// Delete workspaces
		/// </summary>
		/// <param name="toBeDeleted">workspaces to be deleted</param>
		/// <returns>bool to indicate if operation is successfull</returns>
		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> DeleteWorkspaces(GenBOEHomepageWorkspaceRowModelView[] toBeDeleted)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (toBeDeleted == null || !toBeDeleted.Any())
			{
				throw new ArgumentNullException(nameof(toBeDeleted));
			}
			try
			{
				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_HOME_DELETE_WORKSPACES, SecurityPage.WorkspaceDelete, SecurityAuthorization.CreateReadUpdateDelete, GetFullWorkspaces(toBeDeleted), null);

                result.Data = workspaceHomeControllerLogic.DeleteWorkspaces(toBeDeleted); 
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_HOME_DELETE_WORKSPACES, sw);
            }
            catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		/// <summary>
		/// Restore workspace when PTM integrated
		/// </summary>
		/// <param name="toBeRestored">workspace to be restored</param>
		/// <returns>bool to indicate if operation is successfull</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> RestorePtmWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (toBeRestored == null)
			{
				throw new ArgumentNullException(nameof(toBeRestored));
			}

			try
			{
				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE, SecurityPage.WorkspaceRestore, SecurityAuthorization.CreateReadUpdateDelete, GetFullWorkspaces(new[] { toBeRestored }), null);
				
				result.Data = workspaceHomeControllerLogic.RestorePtmWorkspace(toBeRestored);
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		/// <summary>
		/// Restore workspace when not PTM integrated
		/// </summary>
		/// <param name="toBeRestored">workspace to be restored</param>
		/// <returns>bool to indicate if operation is successfull</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> RestoreWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (toBeRestored == null)
			{
				throw new ArgumentNullException(nameof(toBeRestored));
			}

			try
			{
				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_HOME_RESTORE_WORKSPACE, SecurityPage.WorkspaceRestore, SecurityAuthorization.CreateReadUpdateDelete, GetFullWorkspaces(new[] { toBeRestored }), null);
				
				result.Data = workspaceHomeControllerLogic.RestoreWorkspace(toBeRestored);
				result.IsSuccessful = true;             
				
				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_HOME_RESTORE_WORKSPACE, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		/// <summary>
		/// Gets full workspaces from list of workspace view model
		/// </summary>
		/// <param name="workspaces">array of workspace view model</param>
		private ICollection<WorkspaceDTO> GetFullWorkspaces(GenBOEHomepageWorkspaceRowModelView[] workspaces)
		{
			Collection<WorkspaceDTO> fullWorkspaces = new Collection<WorkspaceDTO>();
			foreach (GenBOEHomepageWorkspaceRowModelView workspace in workspaces)
			{
				fullWorkspaces.Add(this.Factory.CreateFullWorkspace(workspace.WorkspaceShortName));
			}
			return fullWorkspaces;
		}
	}
}