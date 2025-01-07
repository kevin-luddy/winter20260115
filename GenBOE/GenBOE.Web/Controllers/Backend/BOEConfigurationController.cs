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
	using System.Linq;
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
		/// Token Handling
		/// </summary>
		private readonly TokenHandling tokenHandler;

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

		public BOEConfigurationController(TokenHandling tokenHandler, ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.tokenHandler = tokenHandler;
		}
		#endregion

		/// <summary>
		/// Get GenBOE web configuration.
		/// </summary>
		/// <returns>GenBOE Configuration Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<BOEConfigurationModelView> GetGenBOEConfigurationData()
		{
			IESResponse<BOEConfigurationModelView> result = new IESResponse<BOEConfigurationModelView>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				result.Data.FirstOrDefault().CompanyConfiguration = SystemConfiguration.Instance().CompanyMode;
				result.Data.FirstOrDefault().IsBRCEnabled = ConfigurationUtilities.GetAppSetting<bool>("EnableBRC");
				result.Data.FirstOrDefault().IsReadOnly = ConfigurationUtilities.GetAppSetting<bool>("IsReadOnly");
				result.Data.FirstOrDefault().DisableExternalLinksForClassifiedInstall = Utilities.DisableExternalHelpLinksForClassifiedInstallations();
				result.Data.FirstOrDefault().EnableSendToProPricerDirectly = ConfigurationUtilities.GetAppSetting<bool>("EnableSendToProPricerDirectly", false);
				result.Data.FirstOrDefault().UnclassifiedBannerText = ConfigurationUtilities.GetAppSetting("UnclassifiedBannerText");
				result.Data.FirstOrDefault().EnableSAP = ConfigurationUtilities.GetAppSetting<bool>("EnableSAP");
				result.Data.FirstOrDefault().ShowEquivalentPersonsOption = ConfigurationUtilities.GetAppSetting<bool>("ShowEquivalentPersonsOption");
				result.Data.FirstOrDefault().IsBOEFormVisible = ConfigurationUtilities.GetAppSetting<bool>("IsBOEFormVisible");

				// Get 'Show IES Header' config value for business areas that aren't RMS (not needed for RMS).
				result.Data.FirstOrDefault().ShowIESHeader = SystemConfiguration.Instance().CompanyMode != CompanyConfiguration.MST && SiteMasterUtilities.ShowIesHeader;

				result.Data.FirstOrDefault().CanCreateWorkspaceWithoutPtmTrackingNumber = ConfigurationUtilities.GetAppSetting<bool>("CanCreateWorkspaceWithoutPtmTrackingNumber");
				result.Data.FirstOrDefault().ArchiveUrlBOE = ConfigurationUtilities.GetAppSetting("ArchiveUrlBoe");
				result.Data.FirstOrDefault().ProductionUrl = ConfigurationUtilities.GetAppSetting("ProdUrlBoe");
				result.Data.FirstOrDefault().ServerUrl = ConfigurationUtilities.GetAppSetting("ServerURL");
				result.Data.FirstOrDefault().IsRMSArchive = SiteMasterUtilities.IsRMSArchive();
				result.Data.FirstOrDefault().IsRMSProduction = SiteMasterUtilities.IsRMSProduction();
				result.Data.FirstOrDefault().IsProjectMapEnabled = ConfigurationUtilities.GetAppSetting<bool>("IsProjectMapEnabled");
				result.Data.FirstOrDefault().MaximumRowsInProjectMap = ConfigurationUtilities.GetAppSetting<int>("MaximumRowsInProjectMap");
				result.Data.FirstOrDefault().ProjectMapPageSize = ConfigurationUtilities.GetAppSetting<int>("ProjectMapPageSize");
				result.Data.FirstOrDefault().ReportServerLocation = ConfigurationUtilities.GetAppSetting("ReportServerLocation");
				result.Data.FirstOrDefault().ReportServerFolderName = ConfigurationUtilities.GetAppSetting("ReportServerFolderName");
				result.Data.FirstOrDefault().ApplicationVersion = ConfigurationUtilities.GetAppSetting("APPLICATION_VERSION");
				result.Data.FirstOrDefault().DisableAllEmails = ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails");
				result.Data.FirstOrDefault().EmailsToCurrentlyLoggedInUser = ConfigurationUtilities.GetAppSetting<bool>("EmailsToCurrentlyLoggedInUser");
				result.Data.FirstOrDefault().ShowUserName = ConfigurationUtilities.GetAppSetting<bool>("ShowUserName");
				result.Data.FirstOrDefault().IsIdentitySwappingAllowed = ConfigurationUtilities.GetAppSetting<bool>("IsIdentitySwappingAllowed");
				result.Data.FirstOrDefault().ShowCompanyConfiguration = ConfigurationUtilities.GetAppSetting<bool>("ShowCompanyConfiguration");

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
		/// <returns>GenBOE Workspace shortspace name.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<WorkspaceSettingsModelView> GetWorkspaceConfiguration(string workspaceShortname)
		{
			IESResponse<WorkspaceSettingsModelView> result = new IESResponse<WorkspaceSettingsModelView>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

				result.Data.FirstOrDefault().IsBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspaceShortname);
				result.Data.FirstOrDefault().IsSAPEnabled = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
				result.Data.FirstOrDefault().IsWorkspaceBeforeSAPCutoff = Utilities.IsWorkspaceBeforeSAPCutoff(ws.CreationDate);
				result.Data.FirstOrDefault().ShowSAPForWorkspace = Utilities.ShowSAPForWorkspace(ws.CreationDate);
				result.Data.FirstOrDefault().IsConfidenceReportEnabled = Utilities.IsConfidenceReportEnabled;
				result.Data.FirstOrDefault().ShowSkillMixForWorkspace = Utilities.ShowSkillMixForWorkspace(ws.CreationDate);
				result.Data.FirstOrDefault().IsHistoricalReferenceExplanationRequired = Utilities.IsHistoricalReferenceExplanationRequired(ws.CreationDate);

				result.IsSuccessful = true;
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