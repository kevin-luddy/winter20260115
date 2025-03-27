namespace GenBOE.ActionLogic.ModelView.Workspace
{
	using System;
	using System.Collections.Generic;
	using IES.Common;

	/// <summary>
	/// This class is created for the genBOE Angular POST payload
	/// </summary>
	public class WorkspaceIdentificationPostBody : IWorkspaceIdentificationModelView
	{
		// Only fields used by all company configurations should be declared here

		/// <summary>
		/// Get/Set the ContainsOCI
		/// </summary>
		public bool ContainsOCI { get; set; }

		/// <summary>
		/// Get/Set the ContractEndDate
		/// </summary>
		public string ContractEndDate { get; set; }

		/// <summary>
		/// Get/Set the ContractStartDate
		/// </summary>
		public string ContractStartDate { get; set; }

		/// <summary>
		/// Get/Set the CostVolumeLeadPricerDisplayName
		/// </summary>
		public string CostVolumeLeadPricerDisplayName { get; set; }

		/// <summary>
		/// Get/Set the CostVolumeLeadPricerNTID
		/// </summary>
		public string CostVolumeLeadPricerNTID { get; set; }

		/// <summary>
		/// Get/Set the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Get/Set the LineOfBusinessTypeID
		/// </summary>
		public int LineOfBusinessTypeID { get; set; }

		/// <summary>
		/// Get/Set the ProposalStatus
		/// </summary>
		public ProposalStatusType ProposalStatus { get; set; }

		/// <summary>
		/// Get/Set the ProposalSubmittalDate
		/// </summary>
		public string ProposalSubmittalDate { get; set; }

		/// <summary>
		/// Get/Set the RFPNumber
		/// </summary>
		public string RFPNumber { get; set; }

		/// <summary>
		/// Get/Set the StatusComments
		/// </summary>
		public string StatusComments { get; set; }

		/// <summary>
		/// Get/Set the WorkspaceID
		/// </summary>
		public int WorkspaceID { get; set; }

		/// <summary>
		/// Get/Set the WorkspaceName
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Line of Business display name for the UI.
		/// </summary>
		public string LineOfBusinessDisplayName { get; set; }

		/// <summary>
		/// Number of decimal digits for labor hours precision.
		/// </summary>
		public int? ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Number of decimal digits for labor costs precision.
		/// </summary>
		public int CostDecimalPrecision { get; set; }

		// methods from parent class PersistedDataModelView
		/// <summary>
		/// Get/Set the UpdateDate
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// Get/Set the UpdateDateLong
		/// </summary>
		public string UpdateDateLong { get; set; }

		/// <summary>
		/// Get/Set the LabelLeadPricer 
		/// </summary>
		public String LabelLeadPricer { get; }

		/// <summary>
		/// Get/Set the ApplicationURL
		/// </summary>
		public Uri ApplicationURL { get; set; }

		/// <summary>
		/// Get/Set the ShortName
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Gets or sets the custom field sorting.
		/// </summary>
		public CustomFieldSorting CustomFieldSorting { get; set; }

		/// <summary>
		/// Gets or sets the resource sorting.
		/// </summary>
		public CustomFieldSorting ResourceSorting { get; set; }

		/// <summary>
		/// Gets or sets the perf org sorting.
		/// </summary>
		public CustomFieldSorting PerfOrgSorting { get; set; }

		/// <summary>
		/// PTM Tracking number
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// Get/Set the RTE Size Limit (number of characters)
		/// </summary>
		public int? RteSizeLimit { get; set; }

		/// <summary>
		/// Get/Set whether using Template BOE
		/// </summary>
		public bool UsingTemplateBoe { get; set; }

		/// <summary>
		/// Get/set whether using Enable SAP Connection is selected.
		/// </summary>
		public bool EnableSAPConnection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not authors are assignable at task level
		/// </summary>
		public bool EnableAssignTaskAuthor { get; set; }

		/// <summary>
		/// Was WS created prior to Boe Templates being enabled
		/// </summary>
		public bool CreatedPriorToBoeTemplates { get; set; }

		/// <summary>
		/// Get or set whether the workspace should be marked as Current for the PTM Tracking Number
		/// </summary>
		public bool CurrentPTMWorkspace { get; set; }

		#region new genBOE UI
		/// <summary>
		/// Hours Label depending on system and workpace preferences
		/// </summary>
		public string HoursLabel { get; set; }

		/// <summary>
		/// Enable SAP for new genBOE
		/// </summary>
		public bool EnableSAP { get; set; }

		/// <summary>
		/// Will SAP show to user for workspace
		/// </summary>
		public bool ShowSAP { get; set; }

		/// <summary>
		/// Check if PTM Tracking Number has multiple workspaces
		/// </summary>
		public bool DoesPTMMultipleWorkspaces { get; set; }

		/// <summary>
		/// Contract Types
		/// </summary>
		public ICollection<int> ContractTypes { get; set; }

		/// <summary>
		/// Selected Contract Types
		/// </summary>
		public ICollection<int> SelectedContractTypes { get; set; }
		#endregion
	}
}
