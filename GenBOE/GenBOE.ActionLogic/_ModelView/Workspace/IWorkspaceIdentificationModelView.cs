// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
	using System;
	using IES.Common;

	public interface IWorkspaceIdentificationModelView
	{
		// Only fields used by all company configurations should be declared here

		/// <summary>
		/// Get/Set the ContainsOCI
		/// </summary>
		bool ContainsOCI { get; set; }

		/// <summary>
		/// Get/Set the ContractEndDate
		/// </summary>
		string ContractEndDate { get; set; }

		/// <summary>
		/// Get/Set the ContractStartDate
		/// </summary>
		string ContractStartDate { get; set; }

		/// <summary>
		/// Get/Set the CostVolumeLeadPricerDisplayName
		/// </summary>
		string CostVolumeLeadPricerDisplayName { get; set; }

		/// <summary>
		/// Get/Set the CostVolumeLeadPricerNTID
		/// </summary>
		string CostVolumeLeadPricerNTID { get; set; }

		/// <summary>
		/// Get/Set the Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Get/Set the LineOfBusinessTypeID
		/// </summary>
		int LineOfBusinessTypeID { get; set; }

		/// <summary>
		/// Get/Set the ProposalStatus
		/// </summary>
		ProposalStatusType ProposalStatus { get; set; }

		/// <summary>
		/// Get/Set the ProposalSubmittalDate
		/// </summary>
		string ProposalSubmittalDate { get; set; }
		
		/// <summary>
		/// Get/Set the RFPNumber
		/// </summary>
		string RFPNumber { get; set; }

		/// <summary>
		/// Get/Set the StatusComments
		/// </summary>
		string StatusComments { get; set; }

		/// <summary>
		/// Get/Set the WorkspaceID
		/// </summary>
		int WorkspaceID { get; set; }

		/// <summary>
		/// Get/Set the WorkspaceName
		/// </summary>
		string WorkspaceName { get; set; }

		/// <summary>
		/// Line of Business display name for the UI.
		/// </summary>
		string LineOfBusinessDisplayName { get; set; }

		/// <summary>
		/// Number of decimal digits for labor hours precision.
		/// </summary>
		int? ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Number of decimal digits for labor costs precision.
		/// </summary>
		int CostDecimalPrecision { get; set; }

		// methods from parent class PersistedDataModelView
		/// <summary>
		/// Get/Set the UpdateDate
		/// </summary>
		DateTime UpdateDate { get; set; }

		/// <summary>
		/// Get/Set the UpdateDateLong
		/// </summary>
		string UpdateDateLong { get; set; }

		/// <summary>
		/// Get/Set the LabelLeadPricer 
		/// </summary>
		String LabelLeadPricer { get; }

		/// <summary>
		/// Get/Set the ApplicationURL
		/// </summary>
		Uri ApplicationURL { get; set; }

		/// <summary>
		/// Get/Set the ShortName
		/// </summary>
		string ShortName { get; set; }

		/// <summary>
		/// Gets or sets the custom field sorting.
		/// </summary>
		CustomFieldSorting CustomFieldSorting { get; set; }

		/// <summary>
		/// Gets or sets the resource sorting.
		/// </summary>
		CustomFieldSorting ResourceSorting { get; set; }

		/// <summary>
		/// Gets or sets the perf org sorting.
		/// </summary>
		CustomFieldSorting PerfOrgSorting { get; set; }

		/// <summary>
		/// PTM Tracking number
		/// </summary>
		string TrackingNumber { get; set; }

		/// <summary>
		/// Get/Set the RTE Size Limit (number of characters)
		/// </summary>
		int? RteSizeLimit { get; set; }

		/// <summary>
		/// Get/Set whether using Template BOE
		/// </summary>
		bool UsingTemplateBoe { get; set; }

		/// <summary>
		/// Get/set whether using Enable SAP Connection is selected.
		/// </summary>
		bool EnableSAPConnection { get; set; }

		/// <summary>
		/// Was WS created prior to Boe Templates being enabled
		/// </summary>
		bool CreatedPriorToBoeTemplates { get; set; }

		/// <summary>
		/// Get or set whether the workspace should be marked as Current for the PTM Tracking Number
		/// </summary>
		bool CurrentPTMWorkspace { get; set; }

		/// <summary>
		/// Get or set the Assign Authors at Task level property
		/// </summary>
		bool EnableAssignTaskAuthor { get; set; }
	}
}
