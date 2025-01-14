// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using IES.Common;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class BOEConfigurationViewModel
	{
		/// <summary>
		/// Company Configuration / Company Mode for the instance.
		/// </summary>
		public CompanyConfiguration CompanyConfiguration { get; set; }

		/// <summary>
		/// Is BRC Enabled for the system configuration?
		/// </summary>
		public bool IsBRCEnabled { get; set; }

		/// <summary>
		/// Is the entire site in Read Only mode?
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Are external links for classified install disabled for the entire system configuration?
		/// </summary>
		public bool DisableExternalLinksForClassifiedInstall { get; set; }

		/// <summary>
		/// Is sending to Pro Pricer directly enabled for the system configuration?
		/// </summary>
		public bool EnableSendToProPricerDirectly { get; set; }

		/// <summary>
		/// The text for the Unclassified Banner.
		/// </summary>
		public string UnclassifiedBannerText { get; set; }

		/// <summary>
		/// Is SAP enabled from the system configuration?
		/// </summary>
		public bool EnableSAP { get; set; }

		/// <summary>
		/// Is the option to show equivalent persons enabled?
		/// </summary>
		public bool ShowEquivalentPersonsOption { get; set; }

		/// <summary>
		/// Is the BOE Form Visible
		/// </summary>
		public bool IsBOEFormVisible { get; set; }

		/// <summary>
		/// Show the IES Header?
		/// </summary>
		public bool ShowIESHeader { get; set; }

		/// <summary>
		/// Can create workspace without PTM Tracking Number?
		/// </summary>
		public bool CanCreateWorkspaceWithoutPtmTrackingNumber { get; set; }

		/// <summary>
		/// The Archive Url BOE configuration value.
		/// </summary>
		public string ArchiveUrlBOE { get; set; }

		/// <summary>
		/// The production url from the configuration.
		/// </summary>
		public string ProductionUrl { get; set; }

		/// <summary>
		/// The server url from the configuration.
		/// </summary>
		public string ServerUrl { get; set; }

		/// <summary>
		/// Is instance RMS Archive from configuration?
		/// </summary>
		public bool IsRMSArchive { get; set; }

		/// <summary>
		/// Is instance RMS Production from configuration?
		/// </summary>
		public bool IsRMSProduction { get; set; }

		/// <summary>
		/// Is Project Map Enabled from configuration?
		/// </summary>
		public bool IsProjectMapEnabled { get; set; }

		/// <summary>
		/// The max amount of rows in project map from configuration.
		/// </summary>
		public int MaximumRowsInProjectMap { get; set; }

		/// <summary>
		/// The page size for the project map.
		/// </summary>
		public int ProjectMapPageSize { get; set; }

		/// <summary>
		/// The report server location.
		/// </summary>
		public string ReportServerLocation {  get; set; }

		/// <summary>
		/// The report server folder name.
		/// </summary>
		public string ReportServerFolderName { get; set; }

		/// <summary>
		/// The application version.
		/// </summary>
		public string ApplicationVersion { get; set; }

		/// <summary>
		/// Option to disable all emails.
		/// </summary>
		public bool DisableAllEmails { get; set; }

		/// <summary>
		/// Option to allow emails to currently logged in user.
		/// </summary>
		public bool EmailsToCurrentlyLoggedInUser { get; set; }

		/// <summary>
		/// Option to show user name.
		/// </summary>
		public bool ShowUserName { get; set; }

		/// <summary>
		/// Option to allow identity swaping.
		/// </summary>
		public bool IsIdentitySwappingAllowed { get; set; }

		/// <summary>
		/// Option to show company configuration (company mode).
		/// </summary>
		public bool ShowCompanyConfiguration { get; set; }
	}
}