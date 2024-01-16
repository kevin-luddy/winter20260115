// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.DTO.Proposal
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using IES.Core;

	/// <summary>
	/// Proposal Dto Class
	/// </summary>
	[Serializable]
	public class ProposalDto : UpdateableDTO, IES.Core.Interfaces.ICachableDTO
	{
		/// <summary>
		/// for ICachableDTO..
		/// </summary>
		/// <returns>Primary Key</returns>
		public int GetPrimaryKeyID()
		{
			return Id;
		}

		/// <summary>
		/// proposal dto class
		/// </summary>
		public ProposalDto()
		{
			Id = -1;
			ProposalTitle = string.Empty;
			TrackingNumber = string.Empty;
			OTISOpportunityID = string.Empty;
			ProposalStatus = ProposalStatus.InProgress;
			ContractTypeIds = new Collection<int>();
			CostElementTypeIds = new Collection<int>();
			UpdateDateAssigned = false;
			ForecastedTrackingNumber = string.Empty;
			IsForecastProposal = false;
			HasWriteAccessToLinkedDocument = false;
		}

		/// <summary>
		/// Proposal Title
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Proposal Tracking Number
		/// This is the unique string that is used for a proposal
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// OTIS Opportunity ID - nullable
		/// </summary>
		public string OTISOpportunityID { get; set; }

		/// <summary>
		/// Proposal Status
		/// </summary>
		public ProposalStatus ProposalStatus { get; set; }

		/// <summary>
		/// Proposal type
		/// </summary>
		public int ProposalType { get; set; }

		/// <summary>
		/// Program Name
		/// </summary>
		public string ProgramName { get; set; }

		/// <summary>
		/// Customer
		/// </summary>
		public string Customer { get; set; }

		/// <summary>
		/// Customer Type
		/// </summary>
		public CustomerType CustomerType { get; set; }

		/// <summary>
		/// ISGS Role
		/// </summary>
		public ISGSRole ISGSRole { get; set; }

		/// <summary>
		/// Proposal Class
		/// </summary>
		public int ProposalClass { get; set; }

		/// <summary>
		/// Request Type
		/// </summary>
		public int? Request { get; set; }

		/// <summary>
		/// RFP Number
		/// </summary>
		public string RFPNumber { get; set; }

		/// <summary>
		/// RFP Issued Date
		/// </summary>
		public DateTime? RFPIssuedDate { get; set; }

		/// <summary>
		/// RFP Received Date
		/// </summary>
		public DateTime? RFPReceivedDate { get; set; }

		/// <summary>
		/// Program Area ID
		/// </summary>
		public int ProgramAreaId { get; set; }

		/// <summary>
		/// Line Of Business ID
		/// </summary>
		public int LineOfBusinessID { get; set; }

		/// <summary>
		/// BOE Tool
		/// </summary>
		public BOETool BoeTool { get; set; }

		/// <summary>
		/// Proposal BOE Tool name user customized for "Other" selection
		/// </summary>
		public string BoeToolName { get; set; }

		/// <summary>
		/// Contract Type Group
		/// </summary>
		public int ContractTypeGroup { get; set; }

		/// <summary>
		/// Contract Type Ids
		/// </summary>
		public ICollection<int> ContractTypeIds { get; set; }

		/// <summary>
		/// Cost Volume Tool
		/// </summary>
		public CostVolumeTool CostVolumeTool { get; set; }

		/// <summary>
		/// Cost Volume Tool Name
		/// </summary>
		public string CostVolumeToolName { get; set; }

		/// <summary>
		/// Anticipated Delivery Date
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Revised Submittal Date
		/// </summary>
		public DateTime? RevisedSubmittalDate { get; set; }

		/// <summary>
		/// Cost Element Type Ids
		/// </summary>
		public ICollection<int> CostElementTypeIds { get; set; }

		/// <summary>
		/// Estimated proposal value
		/// </summary>
		public long? EstimatedProposalValue { get; set; }

		/// <summary>
		/// Proposal Location
		/// </summary>
		public ProposalLocation ProposalLocation { get; set; }

		/// <summary>
		/// Proposal Location Name user customized for "Other" selection
		/// </summary>
		public string ProposalLocationName { get; set; }

		/// <summary>
		/// pricing tool
		/// </summary>
		public PricingTool PricingTool { get; set; }

		/// <summary>
		/// Pricing Tool Name user customized for "Other" selection
		/// </summary>
		public string PricingToolName { get; set; }

		/// <summary>
		/// Is Schedule Proposal
		/// </summary>
		public bool? IsScheduleProposal { get; set; }

		/// <summary>
		/// Date Assigned
		/// </summary>
		public DateTime? DateAssigned { get; set; }

		/// <summary>
		/// Date Created
		/// </summary>
		public DateTime? DateCreated { get; set; }

		/// <summary>
		/// Flag for updating Date Assigned field on upsert
		/// </summary>
		public bool UpdateDateAssigned { get; set; }

		/// <summary>
		/// Comments
		/// </summary>
		public string Comments { get; set; }

		/// <summary>
		/// Created by User ID
		/// </summary>
		public int? CreatedByUserId { get; set; }

		/// <summary>
		/// True if the checklist should change, false otherwise.
		/// </summary>
		public bool ChangeChecklist { get; set; }

		/// <summary>
		/// Program/Proposal Status
		/// </summary>
		public ProgramProposalStatus ProgramProposalStatus { get; set; }

		/// <summary>
		/// Gets or sets the workflow status.
		/// </summary>
		public WorkflowStatus WorkflowStatus { get; set; }

		/// <summary>
		/// Gets or sets the workflow status last updated date.
		/// </summary>
		public DateTime? WorkflowStatusLastUpdated { get; set; }

		/// <summary>
		/// Gets or sets the date when LeadEstimator signed.
		/// </summary>
		public DateTime? LeadEstimatorSignedDate { get; set; }

		/// <summary>
		/// Gets or sets the signature comment for LeadEstimator.
		/// </summary>
		public string LeadEstimatorSignatureComment { get; set; }

		/// <summary>
		/// Gets or sets the date when CoverSheetApprover signed.
		/// </summary>
		public DateTime? CoverSheetApproverSignedDate { get; set; }

		/// <summary>
		/// Gets or sets the signature comment for CoverSheetApprover.
		/// </summary>
		public string CoverSheetApproverSignatureComment { get; set; }

		/// <summary>
		/// Gets or sets the date when PricingVerifier signed.
		/// </summary>
		public DateTime? PricingVerifierSignedDate { get; set; }

		/// <summary>
		/// Gets or sets the signature comment for PricingVerifier.
		/// </summary>
		public string PricingVerifierSignatureComment { get; set; }

		/// <summary>
		/// Gets or sets the date when IndependentReviewer signed.
		/// </summary>
		public DateTime? IndependentReviewerSignedDate { get; set; }

		/// <summary>
		/// Gets or sets the signature comment for IndependentReviewer.
		/// </summary>
		public string IndependentReviewerSignatureComment { get; set; }

		/// <summary>
		/// Gets or sets the date when LOBEstimatingLead signed.
		/// </summary>
		public DateTime? LOBEstimatingLeadSignedDate { get; set; }

		/// <summary>
		/// Gets or sets the signature comment for LOBEstimatingLead.
		/// </summary>
		public string LOBEstimatingLeadSignatureComment { get; set; }

		/// <summary>
		/// Gets or sets the approval email text.
		/// </summary>
		public string ApprovalEmailText { get; set; }

		/// <summary>
		/// Whether or not certified cost and pricing data is required
		/// </summary>
		public bool? IsCCPDRequired { get; set; }

		/// <summary>
		/// Whether or not cost volume is classified
		/// </summary>
		public bool? IsCostVolumeClassified { get; set; }

		/// <summary>
		/// Gets or sets the RDSB document identifier.
		/// </summary>
		public int? DocumentId { get; set; }

		/// <summary>
		/// Forecasted Proposal Tracking Number
		/// This is the unique string that is used for a proposal when it is in the "Forecasted" class.
		/// </summary>
		public string ForecastedTrackingNumber { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is forecast proposal.
		/// </summary>
		public bool IsForecastProposal { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the forecast alert email has been sent.
		/// </summary>
		public bool ForecastEmailSent { get; set; }

		/// <summary>
		/// Does the user have write access to the linked (RDSB) document, for this proposal?
		/// </summary>
		public bool HasWriteAccessToLinkedDocument { get; set; }

		/// <summary>
		/// Gets or sets the agreement date.
		/// </summary>
		public DateTime? AgreementDate { get; set; }

		/// <summary>
		/// Gets or sets the certification date.
		/// </summary>
		public DateTime? CertificationDate { get; set; }

		/// <summary>
		/// Gets or sets the cutoff date utilization.
		/// </summary>
		public CutOffDateUtilization? CutOffDateUtilization { get; set; }

		/// <summary>
		/// Gets or sets the certification timeline completed date.
		/// </summary>
		public DateTime? CertificationTimelineCompleted { get; set; }

		/// <summary>
		/// Gets or sets the proposal completion date.
		/// </summary>
		public DateTime? ProposalCompletedDate { get; set; }

		/// <summary>
		/// Gets or sets selected contract action type
		/// </summary>
		public ContractActionType? ContractActionType { get; set; }

		/// <summary>
		/// Text for when Other is selected for Contract Action Type
		/// </summary>
		public string ContractActionTypeOtherText { get; set; }

		/// <summary>
		/// Gets or sets the certification last emailed date.
		/// </summary>
		public DateTime? CertificationLastEmailed { get; set; }

		/// <summary>
		/// Gets or sets the No Bid date
		/// </summary>
		public DateTime? NoBidDate { get; set; }

		/// <summary>
		/// Gets whether Proposal is a revision
		/// Determined by RevisionOfId being set
		/// </summary>
		public bool IsRevision { get { return RevisionOfId.HasValue; } }

		/// <summary>
		/// ID of the Proposal that this Proposal is a revision of
		/// (null if not a revision)
		/// </summary>
		public int? RevisionOfId { get; set; }

		/// <summary>
		/// Reason why certification is not required
		/// </summary>
		public ReasonCertificationNotRequired? ReasonCertificationNotRequired { get; set; }

		/// <summary>
		/// Comment when "Other" was selected for Certification Not Required Reason
		/// </summary>
		public string OtherReasonComment { get; set; }

		/// <summary>
		/// Comments from the Proposal Setup page
		/// </summary>
		public string ProposalSetupComments { get; set; }

		/// <summary>
		/// Comments from the Manage Proposal Info Page
		/// </summary>
		public string ManageProposalInfoComments { get; set; }

		/// <summary>
		/// The timestamp of the last time this proposal sent Mod Executed Date Reminders
		/// </summary>
		public DateTime? ModExecutedLastEmailed { get; set; }
	}
}
