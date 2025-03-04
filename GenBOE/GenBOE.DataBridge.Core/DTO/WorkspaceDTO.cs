// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using IES.Common.Core.PickList;

	/// <summary>
	/// DTO intended to show a workspace with all its info
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class WorkspaceDTO : UpdateableDTO
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public WorkspaceDTO()
		{
			this.Id = -1;
			WorkspaceName = null;
			ContractStartDate = DateTime.MinValue;
			ContractEndDate = DateTime.MaxValue;
			ProposalSubmittalDate = DateTime.Now;
			RevisedSubmittalDate = null;
			WorkspaceState = WorkspaceState.Initialization;
			TemplateID = -1;
			this.UpdateDate = DateTime.Now;
			ResourceListID = CommonConstants.GLOBAL_LIST_ID; // this needs to be defaulted to 1 so a new workspace will automatically get a copy of the global resource list
			PerfOrgListID = 1; //this needs to be defaulted to 1 so a new workspace will automatically get a copy of the global perf org list
			AllowSearch = false;
			NumberOfTimesExportedToProPricer = 0;
			ProposalStatus = ProposalStatusType.None;
			LineOfBusiness = new PickListDto();
			ProposalClass = new PickListDto();
			SelectedContractTypes = new Collection<int>();
			StatusComment = string.Empty;
			Segment = SegmentType.None;
			HasBeenDeleted = false;
			CostDecimalPrecision = 2;
			IsUsingEquivalentPerson = false;
			ProjectMapType = ProjectMapType.StandardWithoutOffload;
			IsUsingTM = false;
			CustomFieldSorting = CustomFieldSorting.Description;
			ResourceSorting = CustomFieldSorting.Description;
			PerfOrgSorting = CustomFieldSorting.Description;
			EnableSAPConnection = true;
			CurrentPTMWorkspace = false;
			EnableAssignTaskAuthor = false;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is using equivalent person or hours for Labor Values.
		/// </summary>
		public bool IsUsingEquivalentPerson { get; set; }

		/// <summary>
		/// The name of the workspace
		/// </summary>
		public string WorkspaceName { get; set; }

		public string Description { get; set; }
		public string Shortname { get; set; }

		public PickListDto LineOfBusiness { get; set; }
		public PickListDto ProposalClass { get; set; }

		public ICollection<int> SelectedContractTypes { get; set; }
		public SegmentType Segment { get; set; }

		public int CostVolumeLeadPricerUserID { get; set; }

		public DateTime ContractStartDate { get; set; }
		public DateTime ContractEndDate { get; set; }
		public DateTime? ProposalSubmittalDate { get; set; }
		public DateTime? RevisedSubmittalDate { get; set; }
		public string RFPNumber { get; set; }
		public bool ContainsOCI { get; set; }
		public WorkspaceState WorkspaceState { get; set; }
		public int TemplateID { get; set; }

		public int CreatedByUserID { get; set; }

		public int ResourceListID { get; set; }
		public int PerfOrgListID { get; set; }

		public int NumberOfTimesExportedToProPricer { get; set; }

		// Allow searchable workspace
		public bool AllowSearch { get; set; }

		public bool PerfOrgsChanged { get; set; }
		public bool ContainsTemplate { get; set; }
		public string TrackingNumber { get; set; }

		public ProposalStatusType ProposalStatus { get; set; }
		public string StatusComment { get; set; }

		/// <summary>
		/// Numeric value of enumerated type <code>ExportSortBOEBy</code>.
		/// </summary>
		public int BOEExportSortByID { get; set; }

		/// <summary>
		/// When workspace data is imported from genTrac on creation, this is the genTrac Proposal title.
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// True if the workspace is marked for deletion.
		/// </summary>
		public bool HasBeenDeleted { get; set; }

		/// <summary>
		/// Date when the workspace has been soft-deleted (it will be actually deleted 60 days later)
		/// </summary>
		public DateTime? DateDeleted { get; set; }

		/// <summary>
		/// Date when the recalculation has been kicked off for the WS due to precision change.
		/// </summary>
		public DateTime? DateRecalculationStarted { get; set; }

		/// <summary>
		/// Resource hours decimal precision. This is the number of decimal digits to use on resource hour fields (e.g. Labor Spread values.)
		/// </summary>
		public int? ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Resource hours decimal precision (0 if not defined), used for calculations
		/// </summary>
		public int DecimalPrecision
		{
			get
			{
				return ResourceDecimalPrecision ?? 0;
			}
		}

		/// <summary>
		/// Cost decimal precision. This is the number of decimal digits to use on task elements labor spread cost fields. Values 0 or 2.
		/// </summary>
		public int CostDecimalPrecision { get; set; }

		/// <summary>
		/// Gets or sets the Project Map Type
		/// RMS Only, set to None for SSC
		/// </summary>
		public ProjectMapType ProjectMapType { get; set; }

		/// <summary>
		/// Get/Set AllowGridEdit flag (enables/disables in app grid edit functionality)
		/// RMS Only, always false for SSC
		/// </summary>
		public bool AllowGridEdit { get; set; }

		/// <summary>
		/// Gets bool indicating if Workspace is a Project Map Workspace 
		/// RMS only, SSC should always return false
		/// </summary>
		public bool IsProjectMapWorkspace { get { return ProjectMapType == ProjectMapType.TimePhasedProjectMap || ProjectMapType == ProjectMapType.NonTimePhasedProjectMap; } }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is using T&M.
		/// </summary>
		public bool IsUsingTM { get; set; }

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
		/// Gets or sets the last ProPricer instance
		/// </summary>
		public int? LastProPricerInstance { get; set; }

		/// <summary>
		/// Gets or sets the last ProPricer proposal
		/// </summary>
		public string LastProPricerProposal { get; set; }

		/// <summary>
		/// Get/Set the RTE Size Limit (number of characters)
		/// </summary>
		public int? RteSizeLimit { get; set; }

		/// <summary>
		/// This drives MOQ Type usage. Yes -> new, more complex types. No -> legacy / original code.
		/// </summary>
		public bool UsingTemplateBOE { get; set; } = true;

		/// <summary>
		/// Get or set whether using Enable SAP Connection is selected.
		/// </summary>
		public bool EnableSAPConnection { get; set; } = true;

		/// <summary>
		/// Get or set whether the workspace should be marked as Current for the PTM Tracking Number
		/// </summary>
		public bool CurrentPTMWorkspace { get; set; }

		/// <summary>
		/// WS Creation Date
		/// </summary>
		public DateTime? CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the UCOT Factor
		/// </summary>
		public decimal UCOTFactor { get; set; }

		/// <summary>
		/// Gets or sets the Enable Assign Task Author setting
		/// </summary>
		public bool EnableAssignTaskAuthor { get; set; }
	}
}
