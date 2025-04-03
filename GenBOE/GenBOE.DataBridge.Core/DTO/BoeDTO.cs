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
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;

	/// <summary>
	/// DTO that will contain all data for a single BOE
	/// </summary>
	[Serializable()]
	public class BoeDTO : UpdateableDTO, IWorkspaceMembership, ICachableDTO
	{
		private string description;
		private string dataSource;

		public BoeDTO()
		{
			this.Id = -1;
			UpdatedByUserId = -1;
			State = BOEState.Unassigned;
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;
			Title = string.Empty;
			WBSID = null;
			CLINID = null;
			AuthorIDs = new Collection<int>();
			SubcontractorAuthorIDs = new Collection<int>();
			WorkspaceID = 0;
			WCBID = -1;
			HistoricMetricDisclosureChecked = false;
			CustomFieldValueContainers = new Collection<CustomFieldValueContainer>();
			NumAuthorReassigned = 0;
			isMaterial = false;
			IsMultiClinWbs = false;

			description = null;
			dataSource = null;
			WasDataSourceSet = false;
			WasDescriptionSet = false;
			preventRteDbLoad = false;
		}

		public int UpdatedByUserId { get; set; }

		// the BOE state
		public BOEState State { get; set; }

		// the BOE start date
		public DateTime StartDate { get; set; }

		// the BOE end date
		public DateTime EndDate { get; set; }

		// the BOE Title
		public string Title { get; set; }

		// WBS ID associated with the BOE
		public int? WBSID { get; set; }

		// CLIN ID associated with the BOE
		public int? CLINID { get; set; }

		public Collection<int> AuthorIDs { get; set; }

		public Collection<int> SubcontractorAuthorIDs { get; set; }

		// Workspace ID
		public int WorkspaceID { get; set; }

		public DateTime SubmitForApprovalDate { get; set; }

		// ID from WBS_CLIN_BOE_XREF Table
		public int? WCBID { get; set; }

		// the historic metric disclosure statement; true if checked, false if not
		public bool HistoricMetricDisclosureChecked { get; set; }

		// the number of times an author has been reassigned for this BOE
		public int NumAuthorReassigned { get; set; }

		// BOE Custom Fields
		public ICollection<CustomFieldValueContainer> CustomFieldValueContainers { get; set; }

		// is Material BOE or not
		public bool isMaterial { get; set; }

		/// <summary>
		/// Is the boe a multi BOE.
		/// </summary>
		public bool IsMultiClinWbs { get; set; }


		/// <summary>
		/// If this Boe was created from a copy of another Boe, this is the source Boe Id.
		/// </summary>
		public int? CopySourceBoeId { get; set; }

		#region RTE Fields

		/// <summary>
		/// Indicates whether the Description field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasDescriptionSet { get; set; }

		/// <summary>
		/// Indicates whether the Data Source field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasDataSourceSet { get; set; }

		/// <summary>
		/// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
		/// </summary>
		private bool preventRteDbLoad { get; set; }

		// the BOE description
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
				WasDescriptionSet = true;
			}
		}

		// The data source
		public string DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				dataSource = value;
				WasDataSourceSet = true;
			}
		}

		#endregion

		#region Required Validation Messages
		public const string BOE_DESC_REQUIRED = "BOE Description is required.";
		public const string BOE_TITLE_REQUIRED = "BOE Title is required.";
		public const string MOQ_TYPE_REQUIRED = "MOQ Type is required.";
		public const string TASK_DESCRIPTION_REQUIRED = "Task Description is required.";
		public const string MOQ_EQ_REQUIRED = "MOQ Equation is required.";
		public const string MOQ_TEXT_REQUIRED = "{0} is required.";
		public const string DATA_SOURCE_REQUIRED = "Sources of Data is required.";
		public const string RESOURCE_CODE_REQUIRED = "Resource is required.";
		public const string BUSINESS_RESOURCE_CODE_REQUIRED = "Business Resource Code is required.";
		public const string RESOURCE_AND_BUSINESS_RESOURCE_CODE_REQUIRED = "Resource and Business Resource Code is required.";
		public const string PERFORM_ORG_REQUIRED = "Performing Org is required.";
		public const string CLIN_WBS_REQUIRED = "A CLIN and/or WBS is required.";
		public const string TOTAL_LABOR_SPREAD_INVALID = "Total Resource Spread must equal MOQ Equation Total.";
		public const string ONE_TASK_ELEMENT_REQUIRED = "At least one task for an element of cost is required.";
		public const string RESOURCE_TYPE_FOR_TASK_ELEMENT_REQUIRED = "At least one Resource Type is required for a task element.";
		public const string HISTORIC_METRIC_DISCLOSURE_REQUIRED = "Confirmation that the heritage program name may be disclosed is required.";
		public const string BOE_START_DATE_INVALID = "Start Date must be on or after the {0} start date ({1}).";
		public const string BOE_END_DATE_INVALID = "End Date must be on or before the {0} end date ({1}).";
		#endregion Required Validation Messages

		public int GetPrimaryKeyID()
		{
			return this.Id;
		}

		#region Project Map Fields

		/// <summary>
		/// Gets or sets the sow.
		/// </summary>
		public string SOW { get; set; }

		/// <summary>
		/// Gets or sets the sow title.
		/// </summary>
		public string SOWTitle { get; set; }

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the name of the cam.
		/// </summary>
		public string CamName { get; set; }

		/// <summary>
		/// Gets or sets the class of cost.
		/// </summary>
		public ClassOfCost ClassOfCost { get; set; }

		// BOE
		// ClassOfCost -> CF w/ predefined values??

		// Resource TYpe
		// Add Delete -> CF w/ predefined values??

		#endregion Project Map Fields
	}
}