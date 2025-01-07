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
		/// <param name="loader">Workspace loader</param>
		/// <param name="tokenHandler">Token handler</param>
		/// <param name="reportsControllerLogic">Reports controller logic</param>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="boeExporter">BOE exporter</param>
		/// <param name="boeCustomExporter">BOE custom exporter</param>
		/// <param name="workspaceExportFormatDTOLoader">Workspace export format loader</param>
		/// <param name="traceTableExporter">Trace Table data exporter</param>
		/// <param name="boeFormControllerLogic">BOE Form Controller logic</param>
		/// <param name="contractTypeLoader">Pick List loader for Contract Types</param>
		public BOEConfigurationController(TokenHandling tokenHandler, ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.tokenHandler = tokenHandler;
		}
		#endregion

		/// <summary>
		/// Get GenBOE configuration.
		/// </summary>
		/// <returns>GenBOE Configuration Data</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<BOEConfigurationData> GetGenBOEConfigurationData()
		{
			IESResponse<BOEConfigurationData> result = new IESResponse<BOEConfigurationData>();

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
				// TODO Thomas: Warning Banner shown on System/Workspace Email Preferences page?
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

	}
}