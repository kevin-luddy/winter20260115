// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.classes;
	using System;
	using System.Web.Http;

	/// <summary>
	/// BOE Configuration Controller used to communicate with the new front-end of GenBOE.
	/// This controller is used to facilitate configuraiton data.
	/// </summary>
	[AllowAnonymous]
	public class BOEConfigurationController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("BOEConfigurationController");

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenHandler">Token handler</param>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>

		public BOEConfigurationController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{

		}
		#endregion

		/// <summary>
		/// Get GenBOE system config data.
		/// </summary>
		/// <returns>GenBOE System Configuration Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<BOEConfigurationModelView> GetSystemConfiguration()
		{
			IESResponse<BOEConfigurationModelView> result = new IESResponse<BOEConfigurationModelView>();
			BOEConfigurationModelView configurationData = new BOEConfigurationModelView();

			try
			{
				configurationData.CompanyConfiguration = SystemConfiguration.Instance().CompanyMode;
				configurationData.IsBRCEnabled = ConfigurationUtilities.GetAppSetting<bool>("EnableBRC");
				configurationData.IsReadOnly = ConfigurationUtilities.GetAppSetting<bool>("IsReadOnly");
				configurationData.DisableExternalLinksForClassifiedInstall = Utilities.DisableExternalHelpLinksForClassifiedInstallations();
				configurationData.EnableSendToProPricerDirectly = ConfigurationUtilities.GetAppSetting<bool>("EnableSendToProPricerDirectly", false);
				configurationData.UnclassifiedBannerText = ConfigurationUtilities.GetAppSetting("UnclassifiedBannerText");
				configurationData.EnableSAP = ConfigurationUtilities.GetAppSetting<bool>("EnableSAP");
				configurationData.ShowEquivalentPersonsOption = ConfigurationUtilities.GetAppSetting<bool>("ShowEquivalentPersonsOption");
				configurationData.IsBOEFormVisible = ConfigurationUtilities.GetAppSetting<bool>("IsBOEFormVisible");

				// Get 'Show IES Header' config value for business areas that aren't RMS (not needed for RMS).
				configurationData.ShowIESHeader = SystemConfiguration.Instance().CompanyMode != CompanyConfiguration.MST && SiteMasterUtilities.ShowIesHeader;

				configurationData.CanCreateWorkspaceWithoutPtmTrackingNumber = ConfigurationUtilities.GetAppSetting<bool>("CanCreateWorkspaceWithoutPtmTrackingNumber");
				configurationData.ArchiveUrlBOE = ConfigurationUtilities.GetAppSetting("ArchiveUrlBoe");
				configurationData.ProductionUrl = ConfigurationUtilities.GetAppSetting("ProdUrlBoe");
				configurationData.ServerUrl = ConfigurationUtilities.GetAppSetting("ServerURL");
				configurationData.IsRMSArchive = SiteMasterUtilities.IsRMSArchive();
				configurationData.IsRMSProduction = SiteMasterUtilities.IsRMSProduction();
				configurationData.IsProjectMapEnabled = ConfigurationUtilities.GetAppSetting<bool>("IsProjectMapEnabled");
				configurationData.MaximumRowsInProjectMap = ConfigurationUtilities.GetAppSetting<int>("MaximumRowsInProjectMap");
				configurationData.ProjectMapPageSize = ConfigurationUtilities.GetAppSetting<int>("ProjectMapPageSize");
				configurationData.ReportServerLocation = ConfigurationUtilities.GetAppSetting("ReportServerLocation");
				configurationData.ReportServerFolderName = ConfigurationUtilities.GetAppSetting("ReportServerFolderName");
				configurationData.ApplicationVersion = ConfigurationUtilities.GetAppSetting("APPLICATION_VERSION");
				configurationData.DisableAllEmails = ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails");
				configurationData.EmailsToCurrentlyLoggedInUser = ConfigurationUtilities.GetAppSetting<bool>("EmailsToCurrentlyLoggedInUser");
				configurationData.ShowUserName = ConfigurationUtilities.GetAppSetting<bool>("ShowUserName");
				configurationData.IsIdentitySwappingAllowed = ConfigurationUtilities.GetAppSetting<bool>("IsIdentitySwappingAllowed");
				configurationData.ShowCompanyConfiguration = ConfigurationUtilities.GetAppSetting<bool>("ShowCompanyConfiguration");

				result.Data.Add(configurationData);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Gen BOE Configuration data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get workspace data by workspace shortname.
		/// </summary>
		/// <param name="workspaceShortname">Shortspace Name</param>
		/// <returns>GenBOE Workspace level data.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<WorkspaceSettingsModelView> GetWorkspaceConfiguration(string workspaceShortname)
		{
			IESResponse<WorkspaceSettingsModelView> result = new IESResponse<WorkspaceSettingsModelView>();
			WorkspaceSettingsModelView workspaceSettings = new WorkspaceSettingsModelView();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
				SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

				// Only return data if the permissions is at a read level or above.
				if (permission >= SecurityAuthorization.Read)
				{
					workspaceSettings.IsBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspaceShortname);
					workspaceSettings.IsSAPEnabled = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
					workspaceSettings.IsWorkspaceBeforeSAPCutoff = Utilities.IsWorkspaceBeforeSAPCutoff(ws.CreationDate);
					workspaceSettings.ShowSAPForWorkspace = Utilities.ShowSAPForWorkspace(ws.CreationDate);
					workspaceSettings.ShowSkillMixForWorkspace = Utilities.ShowSkillMixForWorkspace(ws.CreationDate);
					workspaceSettings.IsHistoricalReferenceExplanationRequired = Utilities.IsHistoricalReferenceExplanationRequired(ws.CreationDate);

					result.Data.Add(workspaceSettings);
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