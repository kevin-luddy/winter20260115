// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// BOE Configuration Controller used to communicate with the new front-end of GenBOE.
	/// This controller is used to facilitate configuraiton data.
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class BOEConfigurationController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("BOEConfigurationController");

		/// <summary>
		/// Service for BOE Configuration Controller.
		/// </summary>
		private BOEConfigurationControllerLogic BOEConfigurationControllerLogic { get; set; }

		/// <summary>
		/// HomeController Logic
		/// </summary>
		protected IHomeControllerLogic homeControllerLogic { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>

		public BOEConfigurationController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, BOEConfigurationControllerLogic BOEConfigurationControllerLogic, IHomeControllerLogic homeControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.BOEConfigurationControllerLogic = BOEConfigurationControllerLogic;
			this.homeControllerLogic = homeControllerLogic;
		}
		#endregion

		/// <summary>
		/// Get GenBOE system config data.
		/// </summary>
		/// <returns>GenBOE System Configuration Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<BOEConfigurationViewModel> GetSystemConfiguration()
		{
			IESSingleResponse<BOEConfigurationViewModel> result = new IESSingleResponse<BOEConfigurationViewModel>();

			try
			{
				result.Data = BOEConfigurationControllerLogic.GetSystemConfiguration();
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning genBOE Configuration data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get workspace data by workspace shortname.
		/// </summary>
		/// <param name="workspaceShortname">Shortspace Name</param>
		/// <returns>genBOE Workspace level data.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<WorkspaceSettingsViewModel> GetWorkspaceConfiguration(string workspaceShortname)
		{
			IESSingleResponse<WorkspaceSettingsViewModel> result = new IESSingleResponse<WorkspaceSettingsViewModel>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
				SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

				// Only return data if the permissions is at a read level or above.
				if (permission >= SecurityAuthorization.Read)
				{
					result.Data = BOEConfigurationControllerLogic.GetWorkspaceConfiguration(ws, workspaceShortname);
					result.IsSuccessful = true;
				}
				else
				{
					result.Messages.Add($"Insufficient permissions for returning the Workspace data.");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Returns the menu items for HomePageMenu
		/// </summary>
		/// <returns>genBOE Home Master Menu</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<GenBOEMasterMenuItemModelView> GetHomeMasterMenuItems()
		{
			IESResponse<GenBOEMasterMenuItemModelView> result = new IESResponse<GenBOEMasterMenuItemModelView>();

			Collection<GenBOEMasterMenuItemModelView> MenuItems = GenBOEMasterMenuItemModelView.BuildHomeMasterMenuItems();

			GenBOEMasterMenuItemModelView AdminMenuItem = MenuItems.Where(x => x.linkText == "Admin").Single().subMenuItems.Where(y => y.linkText == "Metrics Administration").SingleOrDefault();

			if (AdminMenuItem != null)
			{
				homeControllerLogic.PopulateMetricsAdminCompanySpecificProperties(AdminMenuItem);
			}

			if (Utilities.DisableExternalHelpLinksForClassifiedInstallations())
			{
				foreach (GenBOEMasterMenuItemModelView item in MenuItems.ToList())
				{
					if (item.securityPage == SecurityPage.AboutToolsMenu
						|| item.securityPage == SecurityPage.HelpMenu
						|| item.securityPage == SecurityPage.ContactMenu)
					{
						MenuItems.Remove(item);
					}
				}
			}

			foreach (GenBOEMasterMenuItemModelView menuItem in MenuItems) 
			{
				// For menu items with no sub items, check access
				if (menuItem.subMenuItems.Count == 0)
				{
					// If the user has access to this Security Page (which has a given server action), add the menu item to the Model Views
					// OR if the user has access to a URL link, add the menu item
					if ((menuItem.actionName.Length > 0 && CheckPermission(menuItem.securityPage, null) != SecurityAuthorization.None) ||
					   (menuItem.linkUrl != null && CheckPermission(menuItem.securityPage, null) != SecurityAuthorization.None))
					{
						result.Data.Add(menuItem);
					}
				}
				// If the menu items has sub items, we'll check access on each
				else
				{
					// Create a new model View for the inactive top-level menu item
					GenBOEMasterMenuItemModelView inactiveMenuItem = new GenBOEMasterMenuItemModelView
					{
						linkText = menuItem.linkText
					};

					// Iterate the sub items
					foreach (GenBOEMasterMenuItemModelView subMenuItem in menuItem.subMenuItems)
					{
						// If the user has access to this Security Page (which has a given server action), add the sub menu item to the
						// inactive top-level menu item Model View
						// OR if the user has access to a URL link, add the menu item
						if ((subMenuItem.actionName.Length > 0 && CheckPermission(subMenuItem.securityPage, null) != SecurityAuthorization.None) ||
							(subMenuItem.linkUrl != null && CheckPermission(subMenuItem.securityPage, null) != SecurityAuthorization.None))
						{
							inactiveMenuItem.subMenuItems.Add(subMenuItem);
						}
					}

					// If the inactive top-level menu item has sub items that the user has access to,
					// we'll add the top-level menu item to the Model Views
					if (inactiveMenuItem.subMenuItems.Count > 0)
					{
						result.Data.Add(inactiveMenuItem);
					}
				}
			}

			return result;
		}
	}
}