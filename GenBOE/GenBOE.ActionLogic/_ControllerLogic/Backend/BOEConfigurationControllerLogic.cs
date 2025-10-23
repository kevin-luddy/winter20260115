// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// BOE Configuration controller logic class (service).
	/// </summary>
	public class BOEConfigurationControllerLogic
	{
		/// <summary>
		/// Common data mapper.
		/// </summary>
		private ICommonDataMapper commonDataMapper { get; set; }

		/// <summary>
		/// ctor.
		/// </summary>
		public BOEConfigurationControllerLogic(ICommonDataMapper commonDataMapper)
		{
			this.commonDataMapper = commonDataMapper;
		}

		/// <summary>
		/// Get GenBOE system config data.
		/// </summary>
		/// <returns>GenBOE System Configuration Data</returns>
		public BOEConfigurationViewModel GetSystemConfiguration()
		{
			BOEConfigurationViewModel configurationData = new BOEConfigurationViewModel();

			configurationData.CompanyConfiguration = SystemConfiguration.Instance().CompanyMode;
			configurationData.IsPTMIntegrated = Utilities.IsPTMIntegrated;
			configurationData.IsBRCEnabled = ConfigurationUtilities.GetAppSetting<bool>("EnableBRC");
			configurationData.IsReadOnly = ConfigurationUtilities.GetAppSetting<bool>("IsReadOnly");
			configurationData.DisableExternalLinksForClassifiedInstall = Utilities.DisableExternalHelpLinksForClassifiedInstallations();
			configurationData.EnableSendToProPricerDirectly = ConfigurationUtilities.GetAppSetting<bool>("EnableSendToProPricerDirectly", false);
			configurationData.UnclassifiedBannerText = ConfigurationUtilities.GetAppSetting("UnclassifiedBannerText");
			configurationData.EnableSAP = ConfigurationUtilities.GetAppSetting<bool>("EnableSAP");
			configurationData.ShowEquivalentPersonsOption = ConfigurationUtilities.GetAppSetting<bool>("ShowEquivalentPersonsOption");
			configurationData.IsBOEFormVisible = ConfigurationUtilities.GetAppSetting<bool>("IsBOEFormVisible");

			// Get 'Show IES Header' config value for business areas that aren't RMS (not needed for RMS).
			configurationData.ShowIESHeader = SystemConfiguration.Instance().CompanyMode != CompanyConfiguration.MST && ConfigurationUtilities.GetAppSetting<bool>("ShowIesHeader");

			configurationData.CanCreateWorkspaceWithoutPtmTrackingNumber = ConfigurationUtilities.GetAppSetting<bool>("CanCreateWorkspaceWithoutPtmTrackingNumber");
			configurationData.ArchiveUrlBOE = ConfigurationUtilities.GetAppSetting("ArchiveUrlBoe");
			configurationData.ProductionUrl = ConfigurationUtilities.GetAppSetting("ProdUrlBoe");
			configurationData.ServerUrl = ConfigurationUtilities.GetAppSetting("ServerURL");
			configurationData.IsRMSArchive = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST && ConfigurationUtilities.GetAppSetting("ArchiveUrlBoe") == ConfigurationUtilities.GetAppSetting("ServerURL");
			configurationData.IsRMSProduction = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST && ConfigurationUtilities.GetAppSetting("ProdUrlBoe") == ConfigurationUtilities.GetAppSetting("ServerURL");
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
			configurationData.ActiveApp = ConfigurationUtilities.GetAppSetting("ActiveApp");
			configurationData.ServiceCentralLink = Utilities.ServiceCentralLink();
			configurationData.EnableUCOT = ConfigurationUtilities.GetAppSetting<bool>("EnableUCOT");
			configurationData.UCOTStartDate = ConfigurationUtilities.GetAppSetting("UCOTStartDate");
			configurationData.UcotExcludedWorkspaces = ConfigurationUtilities.GetAppSetting("UcotExcludedWorkspaces");
			configurationData.IsTaskAuthorEnabled = Utilities.IsAssignTaskAuthorEnabledForSystem;
			configurationData.IsLmNavigatorEnabled = Utilities.IsLmNavigatorRteLinkEnabledForSystem;
			configurationData.LmNavigatorUrl = Utilities.LmNavigatorAgentUrl;

			return configurationData;
		}

		/// <summary>
		/// Get workspace data by workspace shortname.
		/// </summary>
		/// <param name="ws">Full Workspace.</param>
		/// <param name="workspaceShortname">Shortspace Name.</param>
		/// <returns>genBOE Workspace level data.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public WorkspaceSettingsViewModel GetWorkspaceConfiguration(WorkspaceDTO ws, string workspaceShortname)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			WorkspaceSettingsViewModel workspaceSettings = new WorkspaceSettingsViewModel();

			workspaceSettings.IsBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspaceShortname);
			workspaceSettings.IsSAPEnabled = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			workspaceSettings.IsWorkspaceBeforeSAPCutoff = Utilities.IsWorkspaceBeforeSAPCutoff(ws.CreationDate);
			workspaceSettings.ShowSAPForWorkspace = Utilities.ShowSAPForWorkspace(ws.CreationDate);
			workspaceSettings.ShowSkillMixForWorkspace = Utilities.ShowSkillMixForWorkspace(ws.CreationDate, ws.Shortname);
			workspaceSettings.IsHistoricalReferenceExplanationRequired = Utilities.IsHistoricalReferenceExplanationRequired(ws.CreationDate);
			workspaceSettings.IsLmNavigatorEnabled = Utilities.IsLmNavigatorRteLinkEnabledForSystem && ws.EnableLmNavigator;

			if (ws.Id == 0)
			{
				// if the workspace doesn't exist .. for example if we are running a system job
				workspaceSettings.ProposalName = "No Workspace";

				workspaceSettings.WorkspaceState = commonDataMapper.getWorkspaceStateName(WorkspaceState.None);

				workspaceSettings.Header = WebConstants.LMPI_LABEL_TEXT;
			}
			else
			{
				workspaceSettings.ProposalName = ws.WorkspaceName;

				workspaceSettings.WorkspaceState = commonDataMapper.getWorkspaceStateName(ws.WorkspaceState);

				workspaceSettings.Header = ws.ContainsOCI ?
					WebConstants.LMPI_OCI_LABEL_TEXT :
					WebConstants.LMPI_LABEL_TEXT;
			}

			return workspaceSettings;
		}
	}
}