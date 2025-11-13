// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Diagnostics.CodeAnalysis;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.ValidationAttributes;
	using IES.Common;

	[ExcludeFromCodeCoverage]
	public class CreateWorkspaceModelView : PersistedDataModelView
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public CreateWorkspaceModelView()
		{
			this.WorkspaceID = -1;
			this.WorkspaceName = null;
			this.Description = string.Empty;
			this.Shortname = string.Empty;
			this.ContractStartDate = "01/1900";
			this.ContractEndDate = "12/2099";
			this.ProposalSubmittalDate = DateTime.Now.ToString("MM/dd/yyyy");
			this.UpdateDate = DateTime.Now;
			this.CostVolumeLeadPricerDisplayName = string.Empty;
			this.RFPNumber = string.Empty;
			this.TrackingNumber = string.Empty;
			this.ContainsOCI = false;
			this.ContainsContentTemplates = false;
			this.ShareAndAllowSearch = false;
			this.WorkspaceToCopyID = -1;
			this.BOEsToCopy = new Collection<int>();
			this.CopyPermissions = false;
			this.CopyTasks = false;
			this.CopyLaborSpreads = false;
			this.WSExactCopy = null;
			this.ProposalTitle = string.Empty;
			this.ResourceDecimalPrecision = null;
			this.CostDecimalPrecision = 2;
			this.IsUsingEquivalentPerson = false;
			this.IsUsingTM = false;
			this.ProjectMapType = ProjectMapType.StandardWithoutOffload;
			this.EnableSAPConnection = true;
			this.CurrentPTMWorkspace = false;
			this.EnableAssignTaskAuthor = false;
			this.EnableLmNavigator = true;
		}

		public override string ToString()
		{
			return base.ToString() +
				", WorkspaceID=" + this.WorkspaceID +
				", WorkspaceName=" + this.WorkspaceName +
				", Description=" + this.Description +
				", Shortname=" + this.Shortname +
				", ContractStartDate=" + this.ContractStartDate +
				", ContractEndDate=" + this.ContractEndDate +
				", ProposalSubmittalDate=" + this.ProposalSubmittalDate +
				", UpdateDate=" + this.UpdateDate +
				", CostVolumeLeadPricerDisplayName=" + this.CostVolumeLeadPricerDisplayName +
				", RFPNumber=" + this.RFPNumber +
				", TrackingNumber=" + this.TrackingNumber +
				", ContainsOCI=" + this.ContainsOCI +
				", ContainsContentTemplates=" + this.ContainsContentTemplates +
				", ShareAndAllowSearch=" + this.ShareAndAllowSearch +
				", WorkspaceToCopyID=" + this.WorkspaceToCopyID +
				", BOEsToCopy=" + this.BOEsToCopy +
				", CopyPermissions=" + this.CopyPermissions +
				", CopyTasks=" + this.CopyTasks +
				", CopyLaborSpreads=" + this.CopyLaborSpreads +
				", WSExactCopy=" + this.WSExactCopy;
		}


		/// <summary>
		/// Maximum length for Workspace Name
		/// </summary>
		public const int MAX_COST_VOLUME_LEAD_PRICER = 100;

		/// <summary>
		/// Maximum length for Workspace Name
		/// </summary>
		public const int MAX_WORKSPACE_NAME = 100;

		/// <summary>
		/// Maximum length for Workspace Name
		/// </summary>
		public const int MAX_WORKSPACE_SHORTNAME = 50;

		/// <summary>
		/// Maximum length for RFP Number
		/// </summary>
		public const int MAX_RFP_NUMBER = 100;

		/// <summary>
		/// Maximum length for Tracking Number
		/// </summary>
		public const int MAX_TRACKING_NUMBER = 100;

		/// <summary>
		/// Maximum length for Proposal Title
		/// </summary>
		public const int MAX_PROPOSAL_TITLE = 100;

		/// <summary>
		/// Maximum length for LOB Name
		/// </summary>
		public const int MAX_LOB_NAME = 50;

		/// <summary>
		/// Maximum length for LOB Long Name
		/// </summary>
		public const int MAX_LOB_LONG_NAME = 100;

		/// <summary>
		/// Maximum length for Version Name
		/// </summary>
		public const int MAX_VERSION_NAME = 50;

		/// <summary>
		/// Maximum length for Description
		/// </summary>
		public const int MAX_DESCRIPTION = 1000;

		/// <summary>
		/// Maximum length for Resource
		/// </summary>
		public const int MAX_RESOURCE = 20;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is using equivalent person (or hours).
		/// </summary>
		public bool IsUsingEquivalentPerson { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is using T&amp;M.
		/// </summary>
		public bool IsUsingTM { get; set; }

		/// <summary>
		/// The ID of the workspace in the database
		/// </summary>
		public int WorkspaceID { get; set; }

		/// <summary>
		/// Number of decimal digits for labor hours precision.
		/// </summary>
		[DisplayName("Resource Decimal Precision")]
		[Range(0, 6, ErrorMessage = "Resource Decimal Precision must be between 0 and 6.")]
		public int? ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Number of decimal digits for labor costs precision.
		/// </summary>

		public int CostDecimalPrecision { get; set; }

		private string _workspaceName = "";
		/// <summary>
		/// The name of the workspace
		/// </summary>
		[Required(ErrorMessage = "Workspace Name is required.")]
		[StringLength(100, ErrorMessage = "Workspace Name must not exceed 100 chars.")]
		[ServerValidation(ErrorMessage = "Workspace Name must be Unique", ValidationToPerform = ValidationType.WorkspaceUniqueName)]
		public string WorkspaceName
		{ 
			get => _workspaceName;
			set => _workspaceName = TruncateString(value,MAX_WORKSPACE_NAME);
		}

		private string _description = "";
		/// <summary>
		/// Get/Set the Description
		/// </summary>
		[StringLength(1000, ErrorMessage = "Description must not exceed 1000 chars.")]
		public string Description
		{
			get => _description;
			set => _description = TruncateString(value, MAX_DESCRIPTION);
		}

		private string _shortname = "";
		/// <summary>
		/// Get/Set the Shortname
		/// </summary>
		[Required(ErrorMessage = "URL is required.")]
		[ServerValidation(ErrorMessage = "URL must be unique", ValidationToPerform = ValidationType.WorkspaceUniqueShortname)]
		// Verifies first character is alphanumeric and Verifies all characters are alphanumeric, -, _, or space
		[RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9-_ ]*$", ErrorMessage = "URL must begin with a letter or number and URL format can only contain letters, numbers, blanks, underscores and hyphens.")]
		[StringLength(21, ErrorMessage = "URL must not exceed 21 chars.")]
		public string Shortname
		{
			get => _shortname;
			set => _shortname = TruncateString(value, MAX_WORKSPACE_SHORTNAME);
		}

		private string _cvlpdn = "";
		/// <summary>
		/// Get/Set the CostVolumeLeadPricerDisplayName
		/// </summary>
		/// 
		public string CostVolumeLeadPricerDisplayName
		{
			get => _cvlpdn;
			set => _cvlpdn = TruncateString(value, MAX_COST_VOLUME_LEAD_PRICER);
		}
		/// <summary>
		/// Get/Set the ContractStartDate
		/// </summary>
		[Required(ErrorMessage = "Contract Start Date is required.")]
		// Allows 01-12 for month, 1999-2000 for year
		[RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Contract Start Date format must be in mm/yyyy format.")]
		public string ContractStartDate { get; set; }

		/// <summary>
		/// Get/Set the ContractEndDate
		/// </summary>
		[Required(ErrorMessage = "Contract End Date is required.")]
		// Allows 01-12 for month, 1999-2000 for year
		[RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Contract End Date format must be in mm/yyyy format.")]
		// Contract End Date must occur after the Contract Start Date.
		public string ContractEndDate { get; set; }

		/// <summary>
		/// Get/Set the ProposalSubmittalDate
		/// </summary>
		/// Allows 01-12 for month, 01-31 for day, 1999-2000 for year
		[RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "Proposal Submittal Date format must be mm/dd/yyyy.")]
		public string ProposalSubmittalDate { get; set; }

		private string _trackingNumber = "";
		/// <summary>
		/// Get/Set the TrackingNumber
		/// </summary>
		[StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the Tracking#")]
		public string TrackingNumber
		{
			get => _trackingNumber;
			set => _trackingNumber = TruncateString(value, MAX_TRACKING_NUMBER);
		}

		private string _rfpNumber = "";
		/// <summary>
		/// Get/Set the RFPNumber
		/// </summary>
		[StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the RFP#")]
		public string RFPNumber
		{
			get => _rfpNumber;
			set => _rfpNumber = TruncateString(value, MAX_RFP_NUMBER);
		}
		/// <summary>
		/// Get/Set the ContainsOCI
		/// </summary>
		[Required(ErrorMessage = "Contains OCI Information is required.")]
		public bool ContainsOCI { get; set; }

		/// <summary>
		/// Get/Set the ContainsContentTemplates
		/// </summary>
		[Required(ErrorMessage = "You must specify whether or not this Workspace contains Content Templates.")]
		public bool ContainsContentTemplates { get; set; }

		/// <summary>
		/// Get/Set the ShareAndAllowSearch
		/// </summary>
		[Required(ErrorMessage = "You must specify whether or not you'd like to share and allow searches on this Workspace.")]
		public bool ShareAndAllowSearch { get; set; }

		/// <summary>
		/// Get/Set the WorkspaceToCopyID
		/// </summary>
		public int WorkspaceToCopyID { get; set; }

		/// <summary>
		/// Get/Set the BOEsToCopy
		/// </summary>
		public Collection<int> BOEsToCopy { get; set; }

		/// <summary>
		/// Get/Set the CopyPermissions
		/// </summary>
		public bool CopyPermissions { get; set; }

		/// <summary>
		/// Get/Set the CopyTasks
		/// </summary>
		public bool CopyTasks { get; set; }

		/// <summary>
		/// Get/Set the CopyLaborSpreads
		/// </summary>
		public bool CopyLaborSpreads { get; set; }

		/// <summary>
		/// Get/Set the WSExactCopy
		/// </summary>
		public Boolean? WSExactCopy { get; set; }

		private string _proposalTitle = "";
		/// <summary>
		/// When import from PTM is the PTM proposals' Title.  Otherwise blank.
		/// </summary>
		public string ProposalTitle
		{
			get => _proposalTitle;
			set => _proposalTitle = TruncateString(value, MAX_PROPOSAL_TITLE);
		}

		/// <summary>
		/// Get/Set the applications url
		/// </summary>
		public Uri ApplicationURL { get; set; }

		/// <summary>
		/// Get/Set the Workspace Type
		/// RMS Only, always None for SSC
		/// </summary>
		public ProjectMapType ProjectMapType { get; set; }

		/// <summary>
		/// Get/Set AllowGridEdit flag (enables/disables in-app grid edit functionality)
		/// RMS Only, always false for SSC
		/// </summary>
		[Required(ErrorMessage = "Allow Grid Edit is required.")]
		public bool AllowGridEdit { get; set; }

		/// <summary>
		/// Get/Set the RTE Size Limit (number of characters)
		/// </summary>
		[Range(100, 100000, ErrorMessage = "Rich Text Editor Character Limit must be between 100 and 100,000.")]
		public int? RteSizeLimit { get; set; }

		/// <summary>
		/// Get/Set whether using Template BOE
		/// </summary>
		[Required(ErrorMessage = "A selection for Template BOE is required.")]
		public bool UsingTemplateBoe { get; set; }

		/// <summary>
		/// Get/set whether using Enable SAP Connection is selected.
		/// </summary>
		[Required(ErrorMessage = "Enable SAP connection selection is required.")]
		public bool EnableSAPConnection { get; set; }

		/// <summary>
		/// Get/set whether the new workspace is Current for PTM
		/// </summary>
		public bool CurrentPTMWorkspace { get; set; }

		/// <summary>
		/// Get/set whether Enable assign task author is selected.
		/// </summary>
		[Required(ErrorMessage = "Enable assign task author is required.")]
		public bool EnableAssignTaskAuthor { get; set; }

		/// <summary>
		/// Get/set whether Enable LM Navigator is selected.
		/// </summary>
		[Required(ErrorMessage = "Enable LM Navigator is required.")]
		public bool EnableLmNavigator { get; set; }

		/// <summary>
		/// TruncateString Utility
		/// </summary>
		/// 
		private string TruncateString(string text, int maxLength)
		{
			if (String.IsNullOrEmpty(text))
			{
				return text;
			}

			return text.Length <= maxLength ? text : text.Substring(0, maxLength);
		}
	}
}
