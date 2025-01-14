// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Web.Http;
	using System.Web.Http.Cors;

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
		/// Ctor
		/// </summary>
		/// <param name="tokenHandler">Token handler</param>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>

		public BOEConfigurationController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, BOEConfigurationControllerLogic BOEConfigurationControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.BOEConfigurationControllerLogic = BOEConfigurationControllerLogic;
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
					result.IsSuccessful = false;
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
	}
}