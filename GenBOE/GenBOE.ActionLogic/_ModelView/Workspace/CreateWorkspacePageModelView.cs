// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using IES.Common.PickList;

	/// <summary>
	/// Model View for Create Workspace page
	/// </summary>
	public class CreateWorkspacePageModelView
	{
		/// <summary>
		/// Gets or sets the project map types.
		/// </summary>
		public ICollection<SelectListItem> ProjectMapTypes { get; set; }

		/// <summary>
		/// Gets or sets the line of business types.
		/// </summary>
		public ICollection<PickListDto> LineOfBusinessTypes { get; set; }

		/// <summary>
		/// Gets or sets the proposal class types.
		/// </summary>
		public ICollection<PickListDto> ProposalClassTypes { get; set; }

		/// <summary>
		/// Gets or sets the contract types.
		/// </summary>
		public ICollection<PickListDto> ContractTypes { get; set; }

		/// <summary>
		/// Gets or sets the tracking numbers.
		/// </summary>
		public ICollection<SelectListItem> TrackingNumbers { get; set; }

		/// <summary>
		/// Gets or sets the application URL.
		/// </summary>
		public Uri ApplicationUrl { get; set; }

		/// <summary>
		/// Gets or sets the cost volume lead pricer ntid.
		/// </summary>
		public string CostVolumeLeadPricerNTID { get; set; }

		/// <summary>
		/// Gets or sets the display name of the cost volume lead pricer.
		/// </summary>
		public string CostVolumeLeadPricerDisplayName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this user is system admin.
		/// </summary>
		public bool IsAdmin { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a PTM Tracking Number is required to create a WS for this user
		/// Should only be possible in high side
		/// </summary>
		public bool PtmTrackingNumberNotRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not the SAP Connection is configured, via the value in the config
		/// </summary>
		public bool IsSAPConnectionEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the new Workspace should be the current workspace for PTM
		/// </summary>
		public bool CurrentPTMWorkspace { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not authors are assignable at task level
		/// </summary>
		public bool IsAuthorAssignableAtTaskLevelEnabled { get; set; }
	}
}
