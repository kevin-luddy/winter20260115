// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Web.Http;
	using System.Web.Http.Cors;

	/// <summary>
	/// Workspace Home Controller for getting workspace home data.
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class WorkspaceHomeController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("BOEConfigurationController");

		/// <summary>
		/// Service for Workspace Home Controller.
		/// </summary>
		private WorkspaceHomeControllerLogic WorkspaceHomeControllerLogic { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="workspaceHomeControllerLogic">Service for workspace home controller</param>
		public WorkspaceHomeController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, WorkspaceHomeControllerLogic workspaceHomeControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.WorkspaceHomeControllerLogic = workspaceHomeControllerLogic;
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
					// Iterate over the static collection of Menu Items defined in GenBOEMasterMenuItemModelView
					ICollection<GenBOEMasterMenuItemModelView> MenuItems = GenBOEMasterMenuItemModelView.BuildSiteMasterMenuItems(ws);

					result.Data = MenuItems;
					result.IsSuccessful = true;
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
		/// Gets the site menu items based on the workspace short name.
		/// </summary>
		/// <param name="workspaceShortname">Shortspace Name</param>
		/// <returns>Workspace menu items.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<WorkspaceMasterViewModel> GetWorkspaceMasterViewData(string workspaceShortname)
		{
			IESSingleResponse<WorkspaceMasterViewModel> result = new IESSingleResponse<WorkspaceMasterViewModel>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
				SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

				// Only return data if the permissions is at a read level or above.
				if (permission >= SecurityAuthorization.Read)
				{
					result.Data = WorkspaceHomeControllerLogic.GetWorkspaceMasterViewData(ws);
					result.IsSuccessful = true;
				}
				else
				{
					result.Messages.Add($"Insufficient permissions for returning Workspace data.");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace data: {ex.Message}");
			}

			return result;
		}
	}
}