// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.ModelView.Workspace
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Diagnostics.CodeAnalysis;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// Workspace Identification (MST) ModelView that extends WorkspaceIdentificationModelView
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class WorkspaceIdentificationMSTModelView : WorkspaceIdentificationModelView, IWorkspaceIdentificationModelView
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public WorkspaceIdentificationMSTModelView()
		{
			this.LineOfBusinessTypeID = 0;
			this.CostVolumeLeadPricerNTID = string.Empty;
			this.ProjectMapType = ProjectMapType.StandardWithoutOffload;
		}

		/// <summary>
		/// Constructor to take in workspace and user DTOs
		/// </summary>
		/// <param name="workspaceDTO">The <see cref="WorkspaceDTO"/> object used to initialize the model view</param>
		/// <param name="costVolumeLeadDTO">The <see cref="costVolumeLeadDTO"/> object used to initialize the model view</param>
		public WorkspaceIdentificationMSTModelView(FullWorkspace workspaceDTO, UserDTO costVolumeLeadDTO)
			: base(workspaceDTO, costVolumeLeadDTO)
		{
			if (workspaceDTO != null)
			{
				this.LineOfBusinessTypeID = workspaceDTO.LineOfBusiness.Id;
				this.ProjectMapType = workspaceDTO.ProjectMapType;
				this.AllowGridEdit = workspaceDTO.AllowGridEdit;
				this.TrackingNumber = workspaceDTO.TrackingNumber;
				this.ProposalTitle = workspaceDTO.ProposalTitle;
			}
			if (costVolumeLeadDTO != null)
			{
				this.CostVolumeLeadPricerNTID = costVolumeLeadDTO.NTID;
			}
		}

		/// <summary>
		/// Gets or sets the Cost volumn lead pricer NTID
		/// </summary>
		[Required(ErrorMessage = "Estimating Lead/Pricer is required.")]
		public string CostVolumeLeadPricerNTID { get; set; }

		/// <summary>
		/// Line Of Business Type
		/// </summary>
		[Range(LineOfBusinessTypeConstants.MSTMinimumSelectableValue, LineOfBusinessTypeConstants.MSTMaximumSelectableValue, ErrorMessage = "Line of Business is required.")]
		public int LineOfBusinessTypeID { get; set; }

		public String LabelLeadPricer
		{
			get
			{
				// We can use the SSC string since the requirements are the same
				return CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC;
			}
		}

		/// <summary>
		/// Gets/Sets Project Map Type
		/// </summary>
		public ProjectMapType ProjectMapType { get; set; }

		/// <summary>
		/// Gets bool indicating if Workspace is a Project Map Workspace
		/// </summary>
		public bool IsProjectMapWorkspace { get { return this.ProjectMapType == ProjectMapType.TimePhasedProjectMap || this.ProjectMapType == ProjectMapType.NonTimePhasedProjectMap; } }

		/// <summary>
		/// Get/Set AllowGridEdit flag (enables/disables in app grid edit funcitonality)
		/// RMS Only, always false for SSC
		/// </summary>
		[Required(ErrorMessage = "Allow Grid Edit is required.")]
		public bool AllowGridEdit { get; set; }

		/// <summary>
		/// PA Number from PLD
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// Gets or sets the user-entered workspace name when PLD is enabled
		/// </summary>
		[StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed for the Workspace Name")]
		public string WorkspaceNameInput { get; set; }

		/// <summary>
		/// Gets/Sets date PLD Proposal was last updated
		/// </summary>
		public DateTime? PldLastUpdateDate { get; set; }

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
