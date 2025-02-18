// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Transactions;
	using System.Web.Configuration;
	using System.Web.Mvc;
	using GenBOE.DataBridge.DTO;
	using GenTRAC.ActionLogic.Email;
	using GenTRAC.ActionLogic.Mediator;
	using GenTRAC.ActionLogic.ModelView;
	using GenTRAC.ActionLogic.ModelView.Proposals;
	using GenTRAC.ActionLogic.Validation;
	using GenTRAC.DataBridge;
	using GenTRAC.DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.Objects;
	using GenTRAC.Objects.FullObject;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	/// <summary>
	/// Proposal controller logic
	/// </summary>
	public class ProposalControllerLogic : GenTRACControllerLogic
	{
		/// <summary>
		/// The logger
		/// </summary>
		private IES.Common.Logger log = new IES.Common.Logger(typeof(ProposalControllerLogic));

		/// <summary>
		/// Validation Methods
		/// </summary>
		private IValidationMethods validationMethods = null;

		/// <summary>
		/// org structure data mapper
		/// </summary>
		private IOrgStructureDataMapper orgStructureDataMapper = null;

		/// <summary>
		/// Permission mediator
		/// </summary>
		private IProposalPermissionMediator proposalPermissionMediator = null;

		/// <summary>
		/// Security Information
		/// </summary>
		private IES.Common.ISecurityInformation securityInformation = null;

		/// <summary>
		/// Cache loader
		/// </summary>
		protected IES.Common.ICacheDataLoader CacheLoader { get; set; }

		/// <summary>
		/// Pick List Mapper
		/// </summary>
		private IPickListMapper pickListMapper = null;

		/// <summary>
		/// User Loader
		/// </summary>
		private IUserLoader userLoader = null;

		/// <summary>
		/// GenBOE Permission Loader
		/// </summary>
		private IPermissionsDTODataLoader genBoePermissionLoader;

		/// <summary>
		/// The workspace dto data loader.
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader;

		/// <summary>
		/// The emailer
		/// </summary>
		private IPtmEmailer emailer;

		/// <summary>
		/// Contracts Loader
		/// </summary>
		private IContractsLoader contractsLoader;

		/// <summary>
		/// Create static Regex object for FreeText.
		/// </summary>
		private static Regex regexFreeText = new Regex(ValidationConstants.FREE_TEXT_FORMAT);

		/// <summary>
		/// the name of the proposal information form, needed for validation
		/// </summary>
		public const string PROPOSAL_INFO_FORM = "proposalInfoForm";

		/// <summary>
		/// The name of the proposal general information data form, needed for validation
		/// </summary>
		public const string PROPOSAL_GENERAL_INFO_FORM = "proposalGeneralInfoForm";

		/// <summary>
		/// The name of the proposal approvals form, needed for validation
		/// </summary>
		public const string PROPOSAL_APPROVALS_FORM = "proposalApprovalsForm";

		/// <summary>
		/// The name of the proposal user info form, needed for validation
		/// </summary>
		public const string PROPOSAL_USER_INFO_FORM = "proposalUserInfoForm";

		/// <summary>
		/// The name of the proposal comments form, needed for validation
		/// </summary>
		public const string PROPOSAL_COMMENTS_FORM = "proposalCommentsForm";

		/// <summary>
		/// Suffix for Proposal Revision Tracking Numbers
		/// </summary>
		public const string REVISION_SUFFIX = "-PR";
		
		/// <summary>
		/// Firm proposal class
		/// </summary>
		public const int FIRM_PROPOSAL_CLASS = 1;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inSecurityAccess">Security Access</param>
		/// <param name="inProposalLoader">Proposal Loader</param>
		/// <param name="inValidationMethods">Validation methods</param>
		/// <param name="inProposalMediator">Proposal Mediator</param>
		/// <param name="inUserMapper">User Mapper</param>
		/// <param name="objectFactory">Object Factory</param>
		/// <param name="inOrgStructureMapper">Org Structure mapper</param>
		/// <param name="inProposalPermissionMediator">Permission mediator</param>
		/// <param name="inSecurityInformation">Security information</param>
		/// <param name="inCacheDataLoader">Cache data loader</param>
		/// <param name="pickListMapper">Pick list mapper</param>
		/// <param name="userLoader">UserLoader</param>
		/// <param name="approvalsLoader">Approvals Loader</param>
		/// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
		/// <param name="inChecklistMediator">Checklist Mediator</param>
		/// <param name="workspaceDTODataLoader">The workspace dto data loader.</param>
		/// <param name="genBoePermissionLoader">The GenBOE Permission Loader</param>
		/// <param name="emailer">The PTM Emailer</param>
		/// <param name="contractsLoader">Contracts loader</param>
		public ProposalControllerLogic(
			ISecurityAccess inSecurityAccess,
			IProposalLoader inProposalLoader,
			IValidationMethods inValidationMethods,
			IProposalMediator inProposalMediator,
			IUserMapper inUserMapper,
			IFullObjectFactory objectFactory,
			IOrgStructureDataMapper inOrgStructureMapper,
			IProposalPermissionMediator inProposalPermissionMediator,
			IES.Common.ISecurityInformation inSecurityInformation,
			IES.Common.ICacheDataLoader inCacheDataLoader,
			IPickListMapper pickListMapper,
			IUserLoader userLoader,
			IApprovalsLoader approvalsLoader,
			IProposalChecklistLoader proposalChecklistLoader,
			IChecklistMediator inChecklistMediator,
			IWorkspaceDTODataLoader workspaceDTODataLoader,
			IPermissionsDTODataLoader genBoePermissionLoader,
			IPtmEmailer emailer,
			IContractsLoader contractsLoader)
			: base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
		{
			this.validationMethods = inValidationMethods;
			this.orgStructureDataMapper = inOrgStructureMapper;
			this.proposalPermissionMediator = inProposalPermissionMediator;
			this.securityInformation = inSecurityInformation;
			this.CacheLoader = inCacheDataLoader;
			this.pickListMapper = pickListMapper;
			this.userLoader = userLoader;
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.genBoePermissionLoader = genBoePermissionLoader;
			this.emailer = emailer;
			this.contractsLoader = contractsLoader;
		}

		/// <summary>
		/// Saves a proposal
		/// </summary>
		/// <param name="proposalInfo">Proposal information model view</param>
		/// <param name="proposalGeneralInfo">Proposal general information model view</param>
		/// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
		/// <param name="proposalUserInfo">Proposal user information model view</param>
		/// <param name="proposalComments">Proposal comments model view</param>
		/// <param name="revisionOfId">ID of the revised Proposal if this is a Revision</param>
		/// <returns>true if success else false</returns>
		public int? SaveProposal(ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
			ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo, ProposalCommentsModelView proposalComments, int? revisionOfId)
		{
			if (proposalInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalInfo));
			}

			if (proposalGeneralInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalGeneralInfo));
			}

			if (proposalUserInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalUserInfo));
			}

			if (proposalComments == null)
			{
				throw new ArgumentNullException(nameof(proposalComments));
			}

			if (proposalApprovalsInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalApprovalsInfo));
			}

			bool shouldSendEmail = false;
			int? proposalId = null;

			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.SaveNewProposal", this.log))
			{
				ProposalPermissionDto pricerPermission = null;
				bool newPricer = this.IsNewPricer(proposalInfo.ProposalID, proposalApprovalsInfo, out pricerPermission);

				// Pricer is required during validation unless this is a forecast proposal
				bool isForecasted = proposalInfo.ProposalClassText == Constants.PROPOSAL_CLASS_FORECASTED;

				// if new proposal, as long as not FORECASTED -> send EMAIL
				shouldSendEmail = proposalInfo.ProposalID == -1 && !isForecasted;
				if (proposalInfo.ProposalID > 0 && !isForecasted)
				{
					ProposalDto existingRecord = ProposalLoader.GetById(proposalInfo.ProposalID);

					if (existingRecord != null)
					{
						// if changing from FORECASTED to not -> send EMAIL
						shouldSendEmail = !string.IsNullOrEmpty(existingRecord.ForecastedTrackingNumber);
					}
				}

				ProposalChecklistType proposalChecklistType = ProposalChecklistType.Default;
				if (ProposalChecklistType.InternationalCommercial.IsActive())
				{
					proposalChecklistType = (proposalInfo.CustomerType == CustomerType.InternationalCommercial) || (proposalInfo.CustomerType == CustomerType.Commercial) || (proposalInfo.CustomerType == CustomerType.StateAndLocal) ? ProposalChecklistType.InternationalCommercial : ProposalChecklistType.Default;
				}

				bool updateChecklist = this.DidChecklistTypeChange(proposalInfo.ProposalID, proposalChecklistType);

				if (isForecasted)
				{
					this.UpdateForecastProposalInformation(proposalInfo);
				}

				ProposalDto newProposal = new ProposalDto()
				{
					Id = proposalInfo.ProposalID,
					TrackingNumber = proposalInfo.ProposalTrackingNumber,
					ForecastedTrackingNumber = proposalInfo.ForecastedTrackingNumber,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalInfo.UpdateDate,
					BoeTool = proposalGeneralInfo.BOETool,
					BoeToolName = (proposalGeneralInfo.BOETool != BOETool.Other) ? string.Empty : proposalGeneralInfo.BOEToolName,
					ContractTypeGroup = proposalInfo.ContractTypeGroup,
					ContractTypeIds = proposalInfo.ContractType,
					CostElementTypeIds = proposalInfo.CostElements,
					CostVolumeTool = proposalGeneralInfo.CostVolumeTool,
					CostVolumeToolName = proposalGeneralInfo.CostVolumeTool != CostVolumeTool.Other ? string.Empty : proposalGeneralInfo.CostVolumeToolName,
					Customer = proposalInfo.Customer,
					CustomerType = proposalInfo.CustomerType,
					DeliveryDate = proposalInfo.AnticipatedDeliveryDate.ToDateTime("MM/dd/yyyy"),
					RevisedSubmittalDate = string.IsNullOrEmpty(proposalInfo.RevisedSubmittalDate) ? null : (DateTime?)proposalInfo.RevisedSubmittalDate.ToDateTime("MM/dd/yyyy"),
					EstimatedProposalValue = string.IsNullOrEmpty(proposalInfo.EstimatedProposalValue) ? null : (long?)long.Parse(proposalInfo.EstimatedProposalValue.Replace(",", string.Empty)),
					ISGSRole = proposalInfo.ISGSRole,
					IsScheduleProposal = proposalInfo.IsScheduleProposal,
					ProgramAreaId = int.Parse(proposalGeneralInfo.ProgramArea),
					OTISOpportunityID = proposalGeneralInfo.OTISOpportunityID,
					ProposalLocation = proposalGeneralInfo.ProposalLocation,
					ProposalLocationName = (proposalGeneralInfo.ProposalLocation != ProposalLocation.Other) ? string.Empty : proposalGeneralInfo.ProposalLocationName,
					PricingTool = proposalGeneralInfo.PricingTool,
					PricingToolName = (proposalGeneralInfo.PricingTool != PricingTool.Other) ? string.Empty : proposalGeneralInfo.PricingToolName,
					LineOfBusinessID = int.Parse(proposalGeneralInfo.LineOfBusiness),
					ProgramName = proposalGeneralInfo.ProgramName,
					ProposalClass = proposalInfo.ProposalClass,
					ProposalStatus = proposalInfo.ProposalStatus,
					ProposalTitle = proposalInfo.ProposalTitle,
					ProposalType = proposalInfo.ProposalType,
					Request = proposalInfo.RequestType,
					RFPNumber = proposalInfo.RFPNumber,
					RFPIssuedDate = string.IsNullOrEmpty(proposalInfo.RFPIssuedDate) ? (DateTime?)null
						: proposalInfo.RFPIssuedDate.ToDateTime("MM/dd/yyyy"),
					RFPReceivedDate = string.IsNullOrEmpty(proposalInfo.RFPReceivedDate) ? (DateTime?)null
						: proposalInfo.RFPReceivedDate.ToDateTime("MM/dd/yyyy"),
					UpdateDateAssigned = newPricer,
					CreatedByUserId = this.UserMapper.GetActiveUser().Id,
					ChangeChecklist = updateChecklist,
					ProgramProposalStatus = proposalGeneralInfo.ProgramProposalStatus,
					IsCCPDRequired = proposalGeneralInfo.IsCCPDRequired,
					IsCostVolumeClassified = proposalGeneralInfo.IsCostVolumeClassified,
					IsForecastProposal = isForecasted,
					DocumentId = proposalInfo.DocumentId,
					RevisionOfId = revisionOfId,
					ProposalSetupComments = proposalComments.Comments,
					ContractActionType = proposalInfo.ContractActionType,
					ContractActionTypeOtherText = proposalInfo.ContractActionTypeOtherText,
					AdditionalClassification = proposalGeneralInfo.AdditionalClassification,
					CcopdNoOtherReason = proposalGeneralInfo.CcopdNoOtherReason,
					CcopdNoReason = proposalGeneralInfo.CcopdNoReason,
					IsSupportOfUndefinitized = proposalGeneralInfo.IsSupportOfUndefinitized
				};

				// copy the old values for approvals/certification (comments, workflow status, signatures, additionalapprovalemailtext)
				if (!isForecasted)
				{
					this.CopyApprovalCertificationData(newProposal, proposalApprovalsInfo);
				}

				proposalId = this.ProposalMediator.SaveProposal(newProposal);

				// If not Forecast proposal
				if (!isForecasted)
				{
					pricerPermission.ProposalID = proposalId;
				}

				if (proposalId.HasValue)
				{
					this.SaveProposalUsers(proposalApprovalsInfo, proposalUserInfo, proposalId.Value, pricerPermission, isForecasted);
				}

				if (updateChecklist)
				{
					// clear the checklist cache
					this.CacheLoader.Remove(IES.Common.CacheConstants.PPR_CHECKLIST_ID_BY_PROPOSAL_ID + proposalId);
					this.CacheLoader.Remove(IES.Common.CacheConstants.PAR_CHECKLIST_ID_BY_PROPOSAL_ID + proposalId);
				}
			}

			if (shouldSendEmail)
			{
				UserDTO contractsLead = this.userLoader.GetByNtid(proposalUserInfo.ContractsPOCNtId);
				UserDTO contractsBackup = this.userLoader.GetByNtid(proposalUserInfo.BackupContractsPOCNtId);
				string lobEstimatingManager = this.userLoader.GetByNtid(proposalApprovalsInfo.LOBEstimatingLeadMgrNtid).DisplayName;
				string leadEstimator = this.userLoader.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid).DisplayName;
				string lob = pickListMapper.GetPickListValues(PickListEnum.LineOfBusiness).PickLists.First(x => x.Id == int.Parse(proposalGeneralInfo.LineOfBusiness)).Text;
				string pa = pickListMapper.GetPickListValues(PickListEnum.ProgramArea).PickLists.First(x => x.Id == int.Parse(proposalGeneralInfo.ProgramArea)).Text;

				if (proposalId.HasValue)
				{
					ProposalDto updatedProposal = ProposalLoader.GetById(proposalId.Value);
					proposalInfo.ProposalTrackingNumber = updatedProposal.TrackingNumber;
					proposalInfo.ProposalID = proposalId.Value;
				}

				this.emailer.SendPtmRecordCreationEmail(this.securityInformation.ActiveUserData, contractsLead, contractsBackup, proposalInfo, WebConfigurationManager.AppSettings["EnableEppIntegration"] == "true", 
															WebConfigurationManager.AppSettings["eEPPUrl"], WebConfigurationManager.AppSettings["ServerURL"], proposalGeneralInfo.ProgramName,
															lob, pa, leadEstimator, proposalUserInfo.CostVolumeLeadDisplayName, lobEstimatingManager, proposalUserInfo.ProposalMgrDisplayName);
			}

			return proposalId;
		}

		/// <summary>
		/// Save only the comments for the proposal
		/// </summary>
		/// <param name="proposalId">ID of Proposal</param>
		/// <param name="proposalComments">Comments</param>
		/// <returns>ID of saved proposal</returns>
		public int? SaveProposalComments(int proposalId, ProposalCommentsModelView proposalComments)
		{
			if (proposalComments == null)
			{
				throw new ArgumentNullException(nameof(proposalComments));
			}

			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.SaveProposalComments", this.log))
			{
				// Get original proposal data
				ProposalDto proposal = this.GetByProposalId(proposalId);

				// update comments
				proposal.ProposalSetupComments = proposalComments.Comments;
				proposal.Updateable = UpdateType.Upsert;

				// save
				return this.ProposalMediator.SaveProposal(proposal);
			}
		}

		/// <summary>
		/// Hard deletes a Proposal
		/// Currently should only be used when revering a revision to a prior version
		/// </summary>
		/// <param name="proposal">Proposal to delete</param>
		public void DeleteProposal(ProposalDto proposal)
		{
			if (proposal == null)
			{
				throw new ArgumentNullException(nameof(proposal));
			}

			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.DeleteProposal", this.log))
			{
				proposal.Updateable = UpdateType.Deleted;
				this.ProposalLoader.Save(proposal);
			}
		}

		/// <summary>
		/// Updates the forecast proposal information before a save.
		/// </summary>
		/// <param name="proposalInfo">The proposal information.</param>
		private void UpdateForecastProposalInformation(ProposalInformationModelView proposalInfo)
		{
			proposalInfo.ProposalTrackingNumber = null;

			if (string.IsNullOrWhiteSpace(proposalInfo.ForecastedTrackingNumber))
			{
				// generate the next Forecasted Tracking Number and sets it
				proposalInfo.ForecastedTrackingNumber = this.ProposalLoader.GetNextForecastedTrackingNumber();
			}
		}

		/// <summary>
		/// Copies the approval data.
		/// </summary>
		/// <param name="newProposal">The new proposal.</param>
		/// <param name="proposalApprovalsInfo">Proposal Approvals Info</param>
		private void CopyApprovalCertificationData(ProposalDto newProposal, ProposalApprovalsModelView proposalApprovalsInfo)
		{
			FullProposal originalProposal = this.GetFullProposalDto(newProposal.Id);
			if (originalProposal != null)
			{
				// Original Approvers
				ProposalPermissionDto originalPricer = originalProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.Pricer);
				ProposalPermissionDto originalCoverSheetApp = originalProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.CoverSheetApprover);
				ProposalPermissionDto originalPricingVer = originalProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.PricingVerification);
				ProposalPermissionDto originalIndRev = originalProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.PeerReviewer);
				ProposalPermissionDto originalLOBEstMgr = originalProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.LOBEstLead);

				// Determine which changed
				bool pricerChanged = (originalPricer != null ? this.UserMapper.GetById(originalPricer.UserId).Ntid : string.Empty) != proposalApprovalsInfo.LeadEstimatorNtid;
				bool coverSheetAppChanged = (originalCoverSheetApp != null ? this.UserMapper.GetById(originalCoverSheetApp.UserId).Ntid : string.Empty) != proposalApprovalsInfo.CoverSheetApproverNtid;
				bool pricingVerChanged = (originalPricingVer != null ? this.UserMapper.GetById(originalPricingVer.UserId).Ntid : string.Empty) != proposalApprovalsInfo.PricingVerificationNtid;
				bool indRevChanged = (originalIndRev != null ? this.UserMapper.GetById(originalIndRev.UserId).Ntid : string.Empty) != proposalApprovalsInfo.IndependentReviewerNtid;
				bool lobEstMgrChanged = (originalLOBEstMgr != null ? this.UserMapper.GetById(originalLOBEstMgr.UserId).Ntid : string.Empty) != proposalApprovalsInfo.LOBEstimatingLeadMgrNtid;

				newProposal.WorkflowStatus = originalProposal.WorkflowStatus;
				newProposal.WorkflowStatusLastUpdated = originalProposal.WorkflowStatusLastUpdated;
				newProposal.LeadEstimatorSignatureComment = pricerChanged ? null : originalProposal.LeadEstimatorSignatureComment;
				newProposal.LeadEstimatorSignedDate = originalProposal.LeadEstimatorSignedDate;
				newProposal.LOBEstimatingLeadSignatureComment = lobEstMgrChanged ? null : originalProposal.LOBEstimatingLeadSignatureComment;
				newProposal.LOBEstimatingLeadSignedDate = originalProposal.LOBEstimatingLeadSignedDate;
				newProposal.PricingVerifierSignatureComment = pricingVerChanged ? null : originalProposal.PricingVerifierSignatureComment;
				newProposal.PricingVerifierSignedDate = originalProposal.PricingVerifierSignedDate;
				newProposal.IndependentReviewerSignatureComment = indRevChanged ? null : originalProposal.IndependentReviewerSignatureComment;
				newProposal.IndependentReviewerSignedDate = originalProposal.IndependentReviewerSignedDate;
				newProposal.AgreementDate = originalProposal.AgreementDate;
				newProposal.CertificationDate = originalProposal.CertificationDate;
				newProposal.CertificationLastEmailed = originalProposal.CertificationLastEmailed;
				newProposal.CertificationTimelineCompleted = originalProposal.CertificationTimelineCompleted;
				newProposal.ProposalCompletedDate = originalProposal.ProposalCompletedDate;
				newProposal.CutOffDateUtilization = originalProposal.CutOffDateUtilization;
				newProposal.Comments = originalProposal.Comments;

				if (newProposal.IsCCPDRequired.HasValue && newProposal.IsCCPDRequired.Value == false)
				{
					// clear out the cover sheet approver
					newProposal.CoverSheetApproverSignatureComment = null;
					newProposal.CoverSheetApproverSignedDate = null;
				}
				else
				{
					newProposal.CoverSheetApproverSignatureComment = coverSheetAppChanged ? null : originalProposal.CoverSheetApproverSignatureComment;
					newProposal.CoverSheetApproverSignedDate = originalProposal.CoverSheetApproverSignedDate;
				}

				newProposal.ApprovalEmailText = pricerChanged ? null : originalProposal.ApprovalEmailText;
			}
		}

		/// <summary>
		/// Determine if the pricer for this proposal has changed
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <param name="proposalApprovals">Proposal approvals model view</param>
		/// <param name="pricerPermission">Pricer permission object</param>
		/// <returns>True if pricer has changed and DateAssigned should be updated</returns>
		internal bool IsNewPricer(int proposalId, ProposalApprovalsModelView proposalApprovals, out ProposalPermissionDto pricerPermission)
		{
			pricerPermission = null;
			bool isNewPricer = false;

			if (proposalApprovals.LeadEstimatorNtid != null)
			{
				UserDTO pricer = UserMapper.GetByNtid(proposalApprovals.LeadEstimatorNtid);
				pricerPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = pricer.Id,
					Role = PtmRole.Pricer,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalApprovals.UpdateDate
				};
			}

			FullProposal fullProposal = this.GetFullProposalDto(proposalId);
			if (fullProposal != null)
			{
				ProposalPermissionDto existingPermission = fullProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.Pricer);
				if (existingPermission != null)
				{
					UserDTO originalPricer = this.UserMapper.GetById(existingPermission.UserId);
					if (originalPricer.Ntid != proposalApprovals.LeadEstimatorNtid)
					{
						// pricer Ntid has changed, set date assigned
						isNewPricer = true;
					}
				}
				else if (!string.IsNullOrWhiteSpace(proposalApprovals.LeadEstimatorNtid))
				{
					// adding new pricer (could happen if going from forecasted to non-forecasted), set date assigned
					isNewPricer = true;
				}
			}
			else
			{
				// new proposal, set date assigned
				isNewPricer = true;
			}

			return isNewPricer;
		}

		/// <summary>
		/// Saves a proposal
		/// </summary>
		/// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
		/// <param name="proposalUserInfo">Proposal user information model view</param>
		/// <param name="proposalId">Proposal Id</param>
		/// <param name="pricerPermission">Pricer permission.  Null if this is a Forecast Proposal</param>
		/// <param name="isForecasted">Whether this is a Forecast Proposal.</param>
		internal void SaveProposalUsers(ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo, int proposalId, ProposalPermissionDto pricerPermission, bool isForecasted)
		{
			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);

			ICollection<ProposalPermissionDto> permissionsToAdd = new List<ProposalPermissionDto>();
			ICollection<ProposalPermissionDto> permissionsToDelete = new List<ProposalPermissionDto>();

			ProposalPermissionDto captureManagerPermission = null;

			if (!isForecasted)
			{
				UserDTO captureManager = UserMapper.GetByNtid(proposalUserInfo.CaptureManagerNtid);
				captureManagerPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = captureManager.Id,
					Role = PtmRole.CaptureManager,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, captureManagerPermission, PtmRole.CaptureManager, permissionsToAdd, permissionsToDelete);

			// cost volume lead
			ProposalPermissionDto costVolumeLeadPermission = null;
			if (!isForecasted)
			{
				UserDTO costVolumeLead = UserMapper.GetByNtid(proposalUserInfo.CostVolumeLeadNtid);
				costVolumeLeadPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = costVolumeLead.Id,
					Role = PtmRole.CostVolumeLead,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, costVolumeLeadPermission, PtmRole.CostVolumeLead, permissionsToAdd, permissionsToDelete);

			// pricer
			this.AddAndDeletePermissions(fullProposalDto, pricerPermission, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto additionalPricingResource1Permission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.AdditionalPricingResource1NtId))
			{
				// additional pricing resource 1
				UserDTO additionalPricingResource1 = UserMapper.GetByNtid(proposalUserInfo.AdditionalPricingResource1NtId);
				additionalPricingResource1Permission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = additionalPricingResource1.Id,
					Role = PtmRole.AdditionalPricingResource1,
					ResourceType = proposalUserInfo.AdditionalPricingResource1Type,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, additionalPricingResource1Permission, PtmRole.AdditionalPricingResource1, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto additionalPricingResource2Permission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.AdditionalPricingResource2NtId))
			{
				// additional pricing resource 2
				UserDTO additionalPricingResource2 = UserMapper.GetByNtid(proposalUserInfo.AdditionalPricingResource2NtId);
				additionalPricingResource2Permission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = additionalPricingResource2.Id,
					Role = PtmRole.AdditionalPricingResource2,
					ResourceType = proposalUserInfo.AdditionalPricingResource2Type,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, additionalPricingResource2Permission, PtmRole.AdditionalPricingResource2, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto backupPricerPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.BackupPricerNtId))
			{
				// backup pricer
				UserDTO backupPricer = UserMapper.GetByNtid(proposalUserInfo.BackupPricerNtId);
				backupPricerPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = backupPricer.Id,
					Role = PtmRole.BackupPricer,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, backupPricerPermission, PtmRole.BackupPricer, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto genBoeWorkspaceCreatorPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.GenBoeWorkspaceCreatorNtid))
			{
				// GenBOE Workspace Creator
				UserDTO genBoeWorkspaceCreator = UserMapper.GetByNtid(proposalUserInfo.GenBoeWorkspaceCreatorNtid);
				genBoeWorkspaceCreatorPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = genBoeWorkspaceCreator.Id,
					Role = PtmRole.GenBoeWorkspaceCreator,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, genBoeWorkspaceCreatorPermission, PtmRole.GenBoeWorkspaceCreator, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto supplyChainPocMatlPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsNtId))
			{
				// supply chain POC (matl)
				UserDTO supplyChainPocMatl = UserMapper.GetByNtid(proposalUserInfo.SupplyChainPOCMaterialsNtId);
				supplyChainPocMatlPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = supplyChainPocMatl.Id,
					Role = PtmRole.SupplyChainPOCMatl,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, supplyChainPocMatlPermission, PtmRole.SupplyChainPOCMatl, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto supplyChainPocMatlBackupPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsBackupNtId))
			{
				// supply chain POC (matl backup)
				UserDTO supplyChainPocMatlBackup = UserMapper.GetByNtid(proposalUserInfo.SupplyChainPOCMaterialsBackupNtId);
				supplyChainPocMatlBackupPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = supplyChainPocMatlBackup.Id,
					Role = PtmRole.BackupMaterialLead,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, supplyChainPocMatlBackupPermission, PtmRole.BackupMaterialLead, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto supplyChainPocSubsPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsNtId))
			{
				// supply chain POC (subs)
				UserDTO supplyChainPocSubs = UserMapper.GetByNtid(proposalUserInfo.SupplyChainPOCSubsNtId);
				supplyChainPocSubsPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = supplyChainPocSubs.Id,
					Role = PtmRole.SupplyChainPOCSubs,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, supplyChainPocSubsPermission, PtmRole.SupplyChainPOCSubs, permissionsToAdd, permissionsToDelete);

			ProposalPermissionDto supplyChainPocSubsBackupPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsBackupNtId))
			{
				// supply chain POC (subs)
				UserDTO supplyChainPocSubsBackup = UserMapper.GetByNtid(proposalUserInfo.SupplyChainPOCSubsBackupNtId);
				supplyChainPocSubsBackupPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = supplyChainPocSubsBackup.Id,
					Role = PtmRole.BackupSubcontractsLead,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, supplyChainPocSubsBackupPermission, PtmRole.BackupSubcontractsLead, permissionsToAdd, permissionsToDelete);

			// Contracts POC manager
			ProposalPermissionDto contractsPocPermission = null;
			if (!isForecasted)
			{
				UserDTO contractsPoc = UserMapper.GetByNtid(proposalUserInfo.ContractsPOCNtId);
				contractsPocPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = contractsPoc.Id,
					Role = PtmRole.ContractsPOC,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, contractsPocPermission, PtmRole.ContractsPOC, permissionsToAdd, permissionsToDelete);

			// Backup Contracts POC manager
			ProposalPermissionDto backupContractsPocPermission = null;
			if (!isForecasted)
			{
				UserDTO backupContractsPoc = UserMapper.GetByNtid(proposalUserInfo.BackupContractsPOCNtId);
				backupContractsPocPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = backupContractsPoc.Id,
					Role = PtmRole.BackupContractsPOC,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, backupContractsPocPermission, PtmRole.BackupContractsPOC, permissionsToAdd, permissionsToDelete);

			// tech lead
			ProposalPermissionDto techLeadPermission = null;
			if (!string.IsNullOrEmpty(proposalUserInfo.TechLeadNtid))
			{
				UserDTO techLead = UserMapper.GetByNtid(proposalUserInfo.TechLeadNtid);
				techLeadPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = techLead.Id,
					Role = PtmRole.TechLead,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, techLeadPermission, PtmRole.TechLead, permissionsToAdd, permissionsToDelete);

			// proposal mgr
			ProposalPermissionDto proposalMgrPermission = null;
			if (!isForecasted)
			{
				UserDTO proposalMgr = UserMapper.GetByNtid(proposalUserInfo.ProposalMgrNtid);
				proposalMgrPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = proposalMgr.Id,
					Role = PtmRole.ProposalMgr,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
			}

			this.AddAndDeletePermissions(fullProposalDto, proposalMgrPermission, PtmRole.ProposalMgr, permissionsToAdd, permissionsToDelete);

			// CoverSheetApprover
			if (string.IsNullOrWhiteSpace(proposalApprovalsInfo.CoverSheetApproverNtid))
			{
				// delete the old coversheet approver permission if any
				this.AddAndDeletePermissions(fullProposalDto, null, PtmRole.CoverSheetApprover, permissionsToAdd, permissionsToDelete);
			}
			else
			{
				UserDTO coverSheetApprover = UserMapper.GetByNtid(proposalApprovalsInfo.CoverSheetApproverNtid);
				ProposalPermissionDto coverSheetApproverPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = coverSheetApprover.Id,
					Role = PtmRole.CoverSheetApprover,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
				this.AddAndDeletePermissions(fullProposalDto, coverSheetApproverPermission, PtmRole.CoverSheetApprover, permissionsToAdd, permissionsToDelete);
			}

			// pricing verification
			if (string.IsNullOrWhiteSpace(proposalApprovalsInfo.PricingVerificationNtid))
			{
				// delete the old pricing verifier permission if any
				this.AddAndDeletePermissions(fullProposalDto, null, PtmRole.PricingVerification, permissionsToAdd, permissionsToDelete);
			}
			else
			{
				UserDTO pricingVerification = UserMapper.GetByNtid(proposalApprovalsInfo.PricingVerificationNtid);
				ProposalPermissionDto pricingVerificationPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = pricingVerification.Id,
					Role = PtmRole.PricingVerification,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
				this.AddAndDeletePermissions(fullProposalDto, pricingVerificationPermission, PtmRole.PricingVerification, permissionsToAdd, permissionsToDelete);
			}

			// independent Reviewer
			if (string.IsNullOrWhiteSpace(proposalApprovalsInfo.IndependentReviewerNtid))
			{
				// delete the old ind. rev. permission if any
				this.AddAndDeletePermissions(fullProposalDto, null, PtmRole.PeerReviewer, permissionsToAdd, permissionsToDelete);
			}
			else
			{
				UserDTO independentReviewer = UserMapper.GetByNtid(proposalApprovalsInfo.IndependentReviewerNtid);
				ProposalPermissionDto independentReviewerPermission = new ProposalPermissionDto()
				{
					Id = -1,
					ProposalID = proposalId,
					UserId = independentReviewer.Id,
					Role = PtmRole.PeerReviewer,
					ResourceType = ResourceType.NotSet,
					Updateable = IES.Common.UpdateType.Upsert,
					UpdateDate = proposalUserInfo.UpdateDate
				};
				this.AddAndDeletePermissions(fullProposalDto, independentReviewerPermission, PtmRole.PeerReviewer, permissionsToAdd, permissionsToDelete);
			}
			// lob estimating lead mgr
			UserDTO lobEstimatingLeadMgr = UserMapper.GetByNtid(proposalApprovalsInfo.LOBEstimatingLeadMgrNtid);
			ProposalPermissionDto lobEstimatingLeadMgrPermission = new ProposalPermissionDto()
			{
				Id = -1,
				ProposalID = proposalId,
				UserId = lobEstimatingLeadMgr.Id,
				Role = PtmRole.LOBEstLead,
				ResourceType = ResourceType.NotSet,
				Updateable = IES.Common.UpdateType.Upsert,
				UpdateDate = proposalUserInfo.UpdateDate
			};
			this.AddAndDeletePermissions(fullProposalDto, lobEstimatingLeadMgrPermission, PtmRole.LOBEstLead, permissionsToAdd, permissionsToDelete);

			this.proposalPermissionMediator.Delete(permissionsToDelete);
			this.proposalPermissionMediator.SaveProposalPermissionDtos(permissionsToAdd);
		}

		/// <summary>
		/// Determines what proposal permissions need to be added and deleted
		/// </summary>
		/// <param name="fullProposal">Full Proposal object</param>
		/// <param name="newPermission">New permission being saved</param>
		/// <param name="role">Role for new permission</param>
		/// <param name="permissionsToAdd">List of permissions to add</param>
		/// <param name="permissionsToDelete">List of permissions to delete</param>
		internal void AddAndDeletePermissions(FullProposal fullProposal, ProposalPermissionDto newPermission, PtmRole role,
			ICollection<ProposalPermissionDto> permissionsToAdd, ICollection<ProposalPermissionDto> permissionsToDelete)
		{
			if (fullProposal != null)
			{
				ProposalPermissionDto existingPermission = fullProposal.Permissions.FirstOrDefault(x => x.Role == role);
				if (existingPermission != null)
				{
					if (newPermission == null)
					{
						// permission existed, new permission is null - delete old
						permissionsToDelete.Add(existingPermission);
					}
					else
					{
						if (existingPermission.UserId != newPermission.UserId ||
							((role == PtmRole.AdditionalPricingResource1 || role == PtmRole.AdditionalPricingResource2) && existingPermission.ResourceType != newPermission.ResourceType))
						{
							// permission existed and was different - delete old and add new
							permissionsToDelete.Add(existingPermission);
							permissionsToAdd.Add(newPermission);
						}
					}
				}
				else
				{
					if (newPermission != null)
					{
						// permission did not exist - add
						permissionsToAdd.Add(newPermission);
					}
				}
			}
			else
			{
				if (newPermission != null)
				{
					// new proposal - always add
					permissionsToAdd.Add(newPermission);
				}
			}
		}

		/// <summary>
		/// Get view for proposal information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <returns>Proposal Index Model View</returns>
		public ProposalIndexModelView GetDataForProposalIndex(int? proposalId)
		{
			ProposalIndexModelView model = new ProposalIndexModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = this.IsProposalReadOnly(proposalId, fullProposalDto);

			// get the US user status
			model.IsUsUser = this.securityInformation.IsDomesticUser(Thread.CurrentPrincipal);

			// Determine visibility status of PSA tab
			model.PsaVisibility = proposalId == null ? SecurityAuthorization.None : this.CheckPermissions(PtmSecurityPage.PostSubmittalAttachments, proposalId).Authorization;

			// Determine visibility status of CertificationTimeline tab
			model.CertificationTimelineVisibility = SecurityAuthorization.None;
			if (fullProposalDto != null && fullProposalDto.IsCCPDRequired.HasValue && fullProposalDto.IsCCPDRequired.Value &&
				(fullProposalDto.ProposalStatus == ProposalStatus.PendingCertification || fullProposalDto.ProposalStatus == ProposalStatus.Revised
				|| fullProposalDto.ProposalStatus == ProposalStatus.Completed || fullProposalDto.ProposalStatus == ProposalStatus.PendingAward || fullProposalDto.ProposalStatus == ProposalStatus.Lost))
			{
				model.CertificationTimelineVisibility = this.CheckPermissions(PtmSecurityPage.CertificationTimeline, proposalId).Authorization;
			}

			model.RevisionHistoryVisibility = SecurityAuthorization.None;
			if (fullProposalDto != null && (fullProposalDto.IsRevision || fullProposalDto.ProposalStatus == ProposalStatus.Revised))
			{
				model.RevisionHistoryVisibility = this.CheckPermissions(PtmSecurityPage.RevisionHistory, proposalId).Authorization;
			}

			model.ContractsVisibility = SecurityAuthorization.None;
			if (fullProposalDto != null)
			{
				model.ContractsVisibility = this.CheckPermissions(PtmSecurityPage.Contracts, proposalId).Authorization;
			}

			if (fullProposalDto != null)
			{
				model.ProposalID = fullProposalDto.Id;
				model.ProposalTrackingNumber = fullProposalDto.TrackingNumber;
				model.ForecastedTrackingNumber = fullProposalDto.ForecastedTrackingNumber;
				model.ProposalTitle = fullProposalDto.ProposalTitle;
				model.AnticipatedDeliveryDate = fullProposalDto.DeliveryDate.ToString("MM/dd/yyyy");
				model.IsCCoPD = fullProposalDto.IsCCPDRequired ?? false;
				if (fullProposalDto.RevisedSubmittalDate.HasValue)
				{
					model.RevisedSubmittalDate = fullProposalDto.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy");
				}

				model.ProposalStatus = fullProposalDto.ProposalStatus;
				DateTime? date = this.ProposalLoader.GetProposalCompletedDate(fullProposalDto.Id);
				model.CompletedDate = date.HasValue ? date.Value.ToString("MM/dd/yyyy") : string.Empty;
				date = fullProposalDto.CertificationTimelineCompleted;
				model.CertificationCompletedDate = date.HasValue ? date.Value.ToString("MM/dd/yyyy") : string.Empty;
				date = fullProposalDto.ProposalCompletedDate;
				model.ProposalCompletedDate = date.HasValue ? date.Value.ToString("MM/dd/yyy") : string.Empty;

				if (!date.HasValue && model.ProposalStatus == ProposalStatus.Completed)
				{
					// assuming all proposals that are completed without certification date were completed before certification was added to PTM
					model.CompletedBeforeCertification = true;
				}

				// Display + New Revision button only if the user if the lead or backup estimator and approval workflow is completed
				bool userIsLeadOrBackupPricer = this.IsCurrentUserPricerOrBackupOrSysAdmin(fullProposalDto.Id);

				model.DisplayNewRevisionButton = fullProposalDto.WorkflowStatus == WorkflowStatus.ProposalLocked && userIsLeadOrBackupPricer;

				// Display Revert to Prior Version button only if user is lead or backup estimator and in latest revision
				if (fullProposalDto.ProposalStatus == ProposalStatus.InProgress && fullProposalDto.IsRevision && userIsLeadOrBackupPricer)
				{
					model.DisplayRevertRevisionButton = true;
				}

				model.ReasonCertificationNotRequired = fullProposalDto.ReasonCertificationNotRequired;

				model.RevisedProposalId = fullProposalDto.RevisionOfId;
				model.IsNewRevision = false;

				model.HasRdsbDocument = fullProposalDto.DocumentId.HasValue;

				// Only get this if revert button is available, since it won't be needed otherwise and we can save a db call
				if (model.DisplayRevertRevisionButton)
				{
					ICollection<GenBOE.Dtos.WorkspaceDTO> workspaces = this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo();
					model.GenBoeWorkspaces = workspaces.Where(x => x.TrackingNumber == fullProposalDto.TrackingNumber).Select(x => x.Shortname).OrderBy(x => x).ToCollection();
				}
			}

			return model;
		}

		/// <summary>
		/// Get view for proposal revision index
		/// </summary>
		/// <param name="proposalId">Proposal Id for proposal being revised</param>
		/// <returns>Proposal Revision Index Model View</returns>
		public ProposalIndexModelView GetDataForProposalRevisionIndex(int proposalId)
		{
			ProposalIndexModelView model = new ProposalIndexModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = "false";

			// get the US user status
			model.IsUsUser = this.securityInformation.IsDomesticUser(Thread.CurrentPrincipal);

			model.PsaVisibility = SecurityAuthorization.None;
			model.CertificationTimelineVisibility = SecurityAuthorization.None;
			model.RevisionHistoryVisibility = SecurityAuthorization.None;
			model.ContractsVisibility = SecurityAuthorization.None;

			if (fullProposalDto != null)
			{
				string newRevisionSuffix;
				string newRevisionTrackingNumber = this.GetRevisionTrackingNumber(fullProposalDto.TrackingNumber, out newRevisionSuffix);

				model.ProposalID = -1;
				model.ProposalTrackingNumber = newRevisionTrackingNumber;
				model.RevisedProposalTitle = fullProposalDto.ProposalTitle;
				model.ProposalTitle = this.GetNewRevisionProposalTitle(fullProposalDto.ProposalTitle, newRevisionSuffix);
				model.ProposalStatus = ProposalStatus.InProgress;
				model.RevisedProposalId = proposalId;
				model.IsNewRevision = true;
			}

			return model;
		}

		/// <summary>
		/// Get view for proposal information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <returns>Proposal Information Model View</returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ProposalInformationModelView GetDataForProposalInformation(int? proposalId)
		{
			ProposalInformationModelView model = new ProposalInformationModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = this.IsProposalReadOnly(proposalId, fullProposalDto);

			bool readOnly = fullProposalDto != null && bool.Parse(model.IsReadOnly);

			model.ContractTypeHtmlOptions = string.Empty;
			model.InactiveContractTypes = string.Empty;
			model.InactiveCostElements = string.Empty;

			if (fullProposalDto != null)
			{
				bool includeIDIQ = fullProposalDto.IsScheduleProposal ?? true;
				// need to populate contract type list based on selected contract type group
				List<PickListDto> selectedInactiveContractTypes = new List<PickListDto>();
				StringBuilder selectList = new StringBuilder();

				if (fullProposalDto.ContractTypeGroup >= 0)
				{
					foreach (PickListDto contractType in this.pickListMapper.GetChildren(PickListEnum.ContractType, fullProposalDto.ContractTypeGroup))
					{
						if (contractType.Text != Constants.IDIQ_CONTRACT_TYPE || includeIDIQ)
						{
							// include if active, or item was selected
							if (contractType.IsActive || fullProposalDto.ContractTypeIds.Contains(contractType.Id))
							{
								bool selected = false;
								if (fullProposalDto.ContractTypeIds.Contains(contractType.Id))
								{
									selected = true;
								}

								selectList.Append(string.Format("<option value=\"{0}\"{2}>{1}</option>",
									contractType.Id, contractType.Text, selected ? " selected=\"selected\"" : string.Empty));
							}

							// Show message for selected inactive contract types
							if (!contractType.IsActive && fullProposalDto != null && fullProposalDto.ContractTypeIds.Contains(contractType.Id))
							{
								selectedInactiveContractTypes.Add(contractType);
							}
						}
					}
				}

				model.ContractTypeHtmlOptions = selectList.ToString();

				if (selectedInactiveContractTypes.Any())
				{
					model.InactiveContractTypes = "The following options are now invalid: <br />" +
						string.Join("<br />", selectedInactiveContractTypes.Select(x => x.Text));
				}
			}

			List<CostElementType> selectedInactiveCostElements = new List<CostElementType>();
			List<CostElementType> enumCostElementList = Enum.GetValues(typeof(CostElementType)).Cast<CostElementType>().ToList();
			model.CostElementsList = new Collection<IES.Common.ListBoxItem>();
			foreach (CostElementType costElementType in enumCostElementList.OrderBy(x => x.ToString()))
			{
				// include if active, or proposal is read only and item was selected
				// use IsEnumActive instead of extension method so it can be mocked in unit tests
				if (this.validationMethods.IsEnumActive(costElementType) || (readOnly && fullProposalDto.CostElementTypeIds.Contains((int)costElementType)))
				{
					model.CostElementsList.Add(new IES.Common.ListBoxItem() { ID = (int)costElementType, Name = costElementType.GetDescription() });
				}
				else if (!this.validationMethods.IsEnumActive(costElementType) && !readOnly && fullProposalDto != null && fullProposalDto.CostElementTypeIds.Contains((int)costElementType))
				{
					selectedInactiveCostElements.Add(costElementType);
				}
			}

			if (selectedInactiveCostElements.Any())
			{
				model.InactiveCostElements = "The following options are now invalid<br />(and will be removed when you save the form): <br />" +
					string.Join("<br />", selectedInactiveCostElements.Select(x => x.GetDescription()));
			}

			if (fullProposalDto != null)
			{
				model.ProposalTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalType, fullProposalDto.ProposalType);
				model.RequestTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.TypeOfRequest, fullProposalDto.Request);
				model.ProposalClassesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalClass, fullProposalDto.ProposalClass);
				model.ContractTypeGroupsList = this.pickListMapper.GetSelectListPickList(PickListEnum.ContractTypeGroup, fullProposalDto.ContractTypeGroup);

				model.ProposalID = fullProposalDto.Id;
				model.ProposalTrackingNumber = fullProposalDto.TrackingNumber;
				model.ForecastedTrackingNumber = fullProposalDto.ForecastedTrackingNumber;
				model.ProposalTitle = fullProposalDto.ProposalTitle;
				model.UpdateDate = fullProposalDto.UpdateDate;
				model.ProposalStatus = fullProposalDto.ProposalStatus;
				model.AnticipatedDeliveryDate = fullProposalDto.DeliveryDate.ToString("MM/dd/yyyy");
				if (fullProposalDto.RevisedSubmittalDate.HasValue)
				{
					model.RevisedSubmittalDate = fullProposalDto.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy");
				}

				model.ContractTypeGroup = fullProposalDto.ContractTypeGroup;
				model.ContractType = fullProposalDto.ContractTypeIds;
				model.CostElements = fullProposalDto.CostElementTypeIds;
				model.Customer = fullProposalDto.Customer;
				model.CustomerType = fullProposalDto.CustomerType;
				model.EstimatedProposalValue = fullProposalDto.EstimatedProposalValue.ToString();
				model.ISGSRole = fullProposalDto.ISGSRole;
				model.IsScheduleProposal = fullProposalDto.IsScheduleProposal;
				model.ProposalType = fullProposalDto.ProposalType;
				model.RequestType = fullProposalDto.Request;
				model.ProposalClass = fullProposalDto.ProposalClass;
				model.RFPNumber = fullProposalDto.RFPNumber;
				if (fullProposalDto.ContractActionType.HasValue)
				{
					model.ContractActionType = (ContractActionType)fullProposalDto.ContractActionType;
				}

				model.ContractActionTypeOtherText = fullProposalDto.ContractActionTypeOtherText;
				model.RFPIssuedDate = fullProposalDto.RFPIssuedDate.HasValue ?
					fullProposalDto.RFPIssuedDate.Value.ToString("MM/dd/yyyy") : string.Empty;
				model.RFPReceivedDate = fullProposalDto.RFPReceivedDate.HasValue ?
					fullProposalDto.RFPReceivedDate.Value.ToString("MM/dd/yyyy") : string.Empty;
				model.ProposalTypeText = model.ProposalTypesList.Any(x => x.Value == model.ProposalType.ToString()) ?
					model.ProposalTypesList.First(x => x.Value == model.ProposalType.ToString()).Text : "Not Set";
				model.ProposalClassText = model.ProposalClassesList.Any(x => x.Value == model.ProposalClass.ToString()) ?
					model.ProposalClassesList.First(x => x.Value == model.ProposalClass.ToString()).Text : "Not Set";
				model.RequestTypeText = model.RequestTypesList.Any(x => x.Value == model.RequestType.ToString()) ?
					model.RequestTypesList.First(x => x.Value == model.RequestType.ToString()).Text : "Not Set";
				model.ContractTypeGroupText = model.ContractTypeGroupsList.Any(x => x.Value == model.ContractTypeGroup.ToString()) ?
					model.ContractTypeGroupsList.First(x => x.Value == model.ContractTypeGroup.ToString()).Text : "Not Set";

				model.DocumentId = fullProposalDto.DocumentId;

			}
			else
			{
				model.ProposalTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalType);
				model.RequestTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.TypeOfRequest);
				model.ProposalClassesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalClass);
				model.ContractTypeGroupsList = this.pickListMapper.GetSelectListPickList(PickListEnum.ContractTypeGroup);
			}

			model.CustomerTypesList = EnumUtilities.GetListItemsForEnumSorted(typeof(CustomerType), true, model.CustomerType.ToString());
			model.ISGSRolesList = EnumUtilities.GetListItemsForEnumSorted(typeof(ISGSRole), true, model.ISGSRole.ToString());
			model.IsNewRevision = false;

			return model;
		}

		/// <summary>
		/// Get view for proposal information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <returns>Proposal Information Model View</returns>
		public ProposalInformationModelView GetDataForProposalRevisionInformation(int? proposalId)
		{
			ProposalInformationModelView model = new ProposalInformationModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = "false";
			bool readOnly = false;

			model.ContractTypeHtmlOptions = string.Empty;
			model.InactiveContractTypes = string.Empty;
			model.InactiveCostElements = string.Empty;

			if (fullProposalDto != null)
			{
				bool includeIDIQ = fullProposalDto.IsScheduleProposal ?? true;
				// need to populate contract type list based on selected contract type group
				List<PickListDto> selectedInactiveContractTypes = new List<PickListDto>();
				StringBuilder selectList = new StringBuilder();

				if (fullProposalDto.ContractTypeGroup >= 0)
				{
					foreach (PickListDto contractType in this.pickListMapper.GetChildren(PickListEnum.ContractType, fullProposalDto.ContractTypeGroup))
					{
						if (contractType.Text != Constants.IDIQ_CONTRACT_TYPE || includeIDIQ)
						{
							// include if active, or item was selected
							if (contractType.IsActive || fullProposalDto.ContractTypeIds.Contains(contractType.Id))
							{
								bool selected = false;
								if (fullProposalDto.ContractTypeIds.Contains(contractType.Id))
								{
									selected = true;
								}

								selectList.Append(string.Format("<option value=\"{0}\"{2}>{1}</option>",
									contractType.Id, contractType.Text, selected ? " selected=\"selected\"" : string.Empty));
							}

							// Show message for selected inactive contract types
							if (!contractType.IsActive && fullProposalDto != null && fullProposalDto.ContractTypeIds.Contains(contractType.Id))
							{
								selectedInactiveContractTypes.Add(contractType);
							}
						}
					}
				}

				model.ContractTypeHtmlOptions = selectList.ToString();

				if (selectedInactiveContractTypes.Any())
				{
					model.InactiveContractTypes = "The following options are now invalid: <br />" +
						string.Join("<br />", selectedInactiveContractTypes.Select(x => x.Text));
				}
			}

			List<CostElementType> selectedInactiveCostElements = new List<CostElementType>();
			List<CostElementType> enumCostElementList = Enum.GetValues(typeof(CostElementType)).Cast<CostElementType>().ToList();
			model.CostElementsList = new Collection<IES.Common.ListBoxItem>();
			foreach (CostElementType costElementType in enumCostElementList.OrderBy(x => x.ToString()))
			{
				// include if active, or proposal is read only and item was selected
				// use IsEnumActive instead of extension method so it can be mocked in unit tests
				if (this.validationMethods.IsEnumActive(costElementType) || (readOnly && fullProposalDto.CostElementTypeIds.Contains((int)costElementType)))
				{
					model.CostElementsList.Add(new IES.Common.ListBoxItem() { ID = (int)costElementType, Name = costElementType.GetDescription() });
				}
				else if (!this.validationMethods.IsEnumActive(costElementType) && !readOnly && fullProposalDto != null && fullProposalDto.CostElementTypeIds.Contains((int)costElementType))
				{
					selectedInactiveCostElements.Add(costElementType);
				}
			}

			if (selectedInactiveCostElements.Any())
			{
				model.InactiveCostElements = "The following options are now invalid<br />(and will be removed when you save the form): <br />" +
					string.Join("<br />", selectedInactiveCostElements.Select(x => x.GetDescription()));
			}

			if (fullProposalDto != null)
			{
				model.ProposalTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalType, fullProposalDto.ProposalType);
				model.RequestTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.TypeOfRequest, fullProposalDto.Request);
				model.ProposalClassesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalClass, fullProposalDto.ProposalClass);
				model.ContractTypeGroupsList = this.pickListMapper.GetSelectListPickList(PickListEnum.ContractTypeGroup, fullProposalDto.ContractTypeGroup);

				string newRevisionSuffix;
				string newRevisionTrackingNumber = this.GetRevisionTrackingNumber(fullProposalDto.TrackingNumber, out newRevisionSuffix);

				model.ProposalID = -1;
				model.ProposalTrackingNumber = newRevisionTrackingNumber;
				model.ProposalTitle = this.GetNewRevisionProposalTitle(fullProposalDto.ProposalTitle, newRevisionSuffix);
				model.ProposalStatus = ProposalStatus.InProgress;

				model.ContractTypeGroup = fullProposalDto.ContractTypeGroup;
				model.ContractType = fullProposalDto.ContractTypeIds;
				model.CostElements = fullProposalDto.CostElementTypeIds;
				model.Customer = fullProposalDto.Customer;
				model.CustomerType = fullProposalDto.CustomerType;
				if (fullProposalDto.ContractActionType.HasValue)
				{
					model.ContractActionType = (ContractActionType)fullProposalDto.ContractActionType;
				}

				model.ContractActionTypeOtherText = fullProposalDto.ContractActionTypeOtherText;
				model.ISGSRole = fullProposalDto.ISGSRole;
				model.IsScheduleProposal = fullProposalDto.IsScheduleProposal;
				model.RequestType = fullProposalDto.Request;
				model.ProposalClass = fullProposalDto.ProposalClass;
				model.ProposalTypeText = "Not Set";
				model.ProposalClassText = model.ProposalClassesList.Any(x => x.Value == model.ProposalClass.ToString()) ?
					model.ProposalClassesList.First(x => x.Value == model.ProposalClass.ToString()).Text : "Not Set";
				model.RequestTypeText = model.RequestTypesList.Any(x => x.Value == model.RequestType.ToString()) ?
					model.RequestTypesList.First(x => x.Value == model.RequestType.ToString()).Text : "Not Set";
				model.ContractTypeGroupText = model.ContractTypeGroupsList.Any(x => x.Value == model.ContractTypeGroup.ToString()) ?
					model.ContractTypeGroupsList.First(x => x.Value == model.ContractTypeGroup.ToString()).Text : "Not Set";

				model.DocumentId = fullProposalDto.DocumentId;

			}
			else
			{
				model.ProposalTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalType);
				model.RequestTypesList = this.pickListMapper.GetSelectListPickList(PickListEnum.TypeOfRequest);
				model.ProposalClassesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalClass);
				model.ContractTypeGroupsList = this.pickListMapper.GetSelectListPickList(PickListEnum.ContractTypeGroup);
			}

			model.CustomerTypesList = EnumUtilities.GetListItemsForEnumSorted(typeof(CustomerType), true, model.CustomerType.ToString());
			model.ISGSRolesList = EnumUtilities.GetListItemsForEnumSorted(typeof(ISGSRole), true, model.ISGSRole.ToString());
			model.IsNewRevision = true;

			return model;
		}

		/// <summary>
		/// Get view for proposal post proposal information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <returns>Proposal Post Proposal Information Model View</returns>
		public ProposalCertificationTimelineModelView GetDataForProposalTimelineCertification(int? proposalId)
		{
			ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);

			if (fullProposalDto != null)
			{
				model.ProposalID = fullProposalDto.Id;
				model.AgreementDate = fullProposalDto.AgreementDate.HasValue ? fullProposalDto.AgreementDate.Value.ToString("MM/dd/yyyy") : string.Empty;
				model.CertificationDate = fullProposalDto.CertificationDate.HasValue ? fullProposalDto.CertificationDate.Value.ToString("MM/dd/yyyy") : string.Empty;

				if (fullProposalDto.CertificationDate.HasValue && fullProposalDto.AgreementDate.HasValue)
				{
					TimeSpan span = fullProposalDto.CertificationDate.Value - fullProposalDto.AgreementDate.Value;
					model.DaysToCertification = Math.Floor(span.TotalDays).ToString();
				}
				else
				{
					model.DaysToCertification = string.Empty;
				}

				model.ReasonCertificationNotRequired = fullProposalDto.ReasonCertificationNotRequired;
				model.OtherReasonCommentCertification = fullProposalDto.OtherReasonComment;
				model.IsReadOnly = this.IsCertificationReadOnly(proposalId ?? -1, fullProposalDto.ProposalStatus, model.ReasonCertificationNotRequired.HasValue);

				model.CutOffDateUtilization = fullProposalDto.CutOffDateUtilization;
				List<SelectListItem> cutoffList = EnumUtilities.GetListItemsForEnumSorted(typeof(CutOffDateUtilization), false, model.CutOffDateUtilization.ToString()).ToList();
				List<SelectListItem> reasonCertificationNotRequiredList = EnumUtilities.GetListItemsForEnumSorted(typeof(ReasonCertificationNotRequired), false, model.ReasonCertificationNotRequired.ToString()).ToList();

				// Add Blank to top of list
				cutoffList.Insert(0, new SelectListItem { Text = string.Empty });
				model.CutOffDateUtilizationList = cutoffList;

				reasonCertificationNotRequiredList.Insert(0, new SelectListItem { Text = string.Empty });
				model.ReasonCertificationNotRequiredList = reasonCertificationNotRequiredList;
				model.Comments = fullProposalDto.Comments;
				model.DisplayCertificationReset = string.Equals(model.IsReadOnly.ToLower(), "true") && model.ReasonCertificationNotRequired.HasValue && this.IsCurrentUserPricerOrBackupOrSysAdmin(fullProposalDto.Id);
				model.DisableCompleteButton = fullProposalDto.ProposalStatus == ProposalStatus.Completed;
				model.DisableCertificationReset = fullProposalDto.ProposalStatus == ProposalStatus.Completed || fullProposalDto.ProposalStatus == ProposalStatus.Lost;
			}

			return model;
		}

		/// <summary>
		/// Get view for proposal general information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <param name="isNewRevision">Whether creating a new revision</param>
		/// <returns>Proposal General Information Model View</returns>
		/// SUPPRESSION NOTE: Different namespaces are used to put together the modelview
		public ProposalGeneralInformationModelView GetDataForProposalGeneralInformation(int? proposalId, bool isNewRevision)
		{
			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			ProposalGeneralInformationModelView model = new ProposalGeneralInformationModelView();

			int? lobId = null;
			if (fullProposalDto != null)
			{
				lobId = fullProposalDto.LineOfBusinessID;
			}

			model.LinesOfBusinessList = this.pickListMapper.GetSelectListPickList(PickListEnum.LineOfBusiness, lobId);

			model.PricingToolsList = EnumUtilities.GetListItemsForEnumSorted(typeof(PricingTool), false, model.PricingTool.ToString());
			model.BOEToolsList = EnumUtilities.GetListItemsForEnumSorted(typeof(BOETool), false, model.BOETool.ToString());
			model.CostVolumeToolsList = EnumUtilities.GetListItemsForEnumSorted(typeof(CostVolumeTool), false, model.CostVolumeTool.ToString());
			model.ReasonsForCcopdBeingNo = EnumUtilities.GetListItemsForEnumSorted(typeof(CcopdOptionalReason), false, model.CcopdNoReason.HasValue ? model.CcopdNoReason.ToString() : string.Empty);

			model.IsReadOnly = isNewRevision ? false.ToString().ToLower() : this.IsProposalReadOnly(proposalId, fullProposalDto);
			model.IsPTMChecklistUIEnabled = (!proposalId.HasValue || proposalId < 0 || fullProposalDto.ProposalChecklistPPRData == null) ? true : this.IsPTMChecklistUIEnabled(fullProposalDto.ProposalChecklistPPRData.Version);

			if (fullProposalDto != null)
			{
				model.ProposalID = fullProposalDto.Id;
				model.BOETool = fullProposalDto.BoeTool;
				model.BOEToolName = fullProposalDto.BoeToolName;
				model.CostVolumeTool = fullProposalDto.CostVolumeTool;
				model.CostVolumeToolName = fullProposalDto.CostVolumeToolName;
				model.ProgramArea = fullProposalDto.ProgramAreaId.ToString();
				model.OTISOpportunityID = fullProposalDto.OTISOpportunityID;
				model.ProposalLocation = fullProposalDto.ProposalLocation;
				model.ProposalLocationName = fullProposalDto.ProposalLocationName;
				model.PricingTool = fullProposalDto.PricingTool;
				model.PricingToolName = fullProposalDto.PricingToolName;
				model.ProgramName = fullProposalDto.ProgramName;
				model.UpdateDate = fullProposalDto.UpdateDate;
				model.ProgramProposalStatus = fullProposalDto.ProgramProposalStatus;
				model.IsCCPDRequired = isNewRevision ? null : fullProposalDto.IsCCPDRequired;
				model.IsCostVolumeClassified = fullProposalDto.IsCostVolumeClassified;
				model.AdditionalClassification = fullProposalDto.AdditionalClassification;
				model.IsCCPDReadOnly = isNewRevision ? false : fullProposalDto.LeadEstimatorSignedDate.HasValue;
				model.CcopdNoOtherReason = fullProposalDto.CcopdNoOtherReason;
				model.CcopdNoReason = fullProposalDto.CcopdNoReason;
				model.IsSupportOfUndefinitized = fullProposalDto.IsSupportOfUndefinitized;

				// Automatically adds selected option, even if the option is not active.
				model.ProposalLocationsList = EnumUtilities.GetListItemsForEnumSorted(typeof(ProposalLocation), false, model.ProposalLocation.ToString());

				model.LineOfBusinessSelectedText = this.pickListMapper.GetById(PickListEnum.LineOfBusiness, fullProposalDto.LineOfBusinessID).Text;
				model.ProgramAreaSelectedText = this.pickListMapper.GetById(PickListEnum.ProgramArea, fullProposalDto.ProgramAreaId).Text;

				model.LineOfBusiness = fullProposalDto.LineOfBusinessID.ToString();

				model.ProgramAreaHtmlOptions = this.GetProgramAreasForLineOfBusiness(fullProposalDto.LineOfBusinessID, fullProposalDto.ProgramAreaId);
			}
			else
			{
				model.ProposalLocationsList = EnumUtilities.GetListItemsForEnumSorted(typeof(ProposalLocation), false);
				model.ProgramAreaHtmlOptions = this.GetProgramAreasForLineOfBusiness(null, null);
			}

			// get dynamic help text for Program Areas
			model.ProgramAreaHelpText = this.orgStructureDataMapper.GetProgramAreaDynamicHelpText();

			return model;
		}

		/// <summary>
		/// GetUsersForSelectList
		/// </summary>
		/// <param name="role">role </param>
		/// <returns>SelectList</returns>
		private ICollection<UserDTO> GetUsersForSelectList(PtmRole role)
		{
			string groupNames = string.Empty;
			string[] groupNamesArray = null;
			ICollection<UserDTO> users = new Collection<UserDTO>();

			// get ad user group from config based on role, return select list
			switch (role)
			{
				case PtmRole.Pricer:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("LeadEstimators");
					break;
				case PtmRole.LOBEstMgr:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("LOBManagers");
					break;
				case PtmRole.PeerReviewer:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("IndependentReviewers");
					break;
				case PtmRole.PricingVerification:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("PricingVerifications");
					break;
				case PtmRole.CoverSheetApprover:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("CoverSheetApprovers");
					break;
				case PtmRole.ContractsPOC:
				case PtmRole.BackupContractsPOC:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("ContractsLead");
					break;
				case PtmRole.SupplyChainPOCMatl:
				case PtmRole.BackupMaterialLead:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("MaterialsLead");
					break;
				case PtmRole.SupplyChainPOCSubs:
				case PtmRole.BackupSubcontractsLead:
					groupNames = IES.Common.ConfigurationUtilities.GetAppSetting("SubcontractsLead");
					break;
			}

			if (groupNames.Contains(","))
			{
				groupNamesArray = groupNames.Split(',');
			}
			else
			{
				groupNamesArray = new string[] { groupNames };
			}

			foreach (string groupName in groupNamesArray)
			{
				users.AddRange(userLoader.GetUserDTOsByADGroup(groupName.GetObjectName()));
			}

			return users.Distinct().ToCollection();
		}

		/// <summary>
		/// Get view data for proposal approval users
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <param name="isNewRevision">Whether creating a new revision</param>
		/// <returns>Proposal Approvals Model View</returns>
		public virtual ProposalApprovalsModelView GetDataForProposalApprovals(int? proposalId, bool isNewRevision)
		{
			ProposalApprovalsModelView model = new ProposalApprovalsModelView();

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = isNewRevision ? "false" : this.IsProposalReadOnly(proposalId, fullProposalDto);

			// get the selection lists based on adusergroups found in web.config for specified users
			model.PricingVerificationList = this.GetUsersForSelectList(PtmRole.PricingVerification);
			model.LOBEstimatingMgrList = this.GetUsersForSelectList(PtmRole.LOBEstMgr);
			model.CoverSheetApproverList = this.GetUsersForSelectList(PtmRole.CoverSheetApprover);
			model.LeadEstimatorList = this.GetUsersForSelectList(PtmRole.Pricer);
			model.IndependentReviewerList = this.GetUsersForSelectList(PtmRole.PeerReviewer);

			if (fullProposalDto != null)
			{
				model.IsCCPDRequired = fullProposalDto.IsCCPDRequired;

				model.UpdateDate = fullProposalDto.UpdateDate;

				ICollection<ProposalPermissionDto> permissions = fullProposalDto.Permissions;

				foreach (ProposalPermissionDto permission in permissions)
				{
					UserDTO user = this.UserMapper.GetById(permission.UserId);
					switch (permission.Role)
					{
						case PtmRole.Pricer:
							model.LeadEstimatorNtid = user.Ntid;
							model.LeadEstimatorDisplayName = user.DisplayName;
							model.IsLeadEstimatorReadOnly = isNewRevision ? false : fullProposalDto.LeadEstimatorSignedDate.HasValue;

							if (!model.LeadEstimatorList.Any(x => x.Ntid == user.Ntid))
							{
								model.LeadEstimatorList.Add(user);
							}

							break;
						case PtmRole.PricingVerification:
							model.PricingVerificationNtid = user.Ntid;
							model.PricingVerificationDisplayName = user.DisplayName;
							model.IsPricingVerificationReadOnly = isNewRevision ? false : fullProposalDto.PricingVerifierSignedDate.HasValue;

							if (!model.PricingVerificationList.Any(x => x.Ntid == user.Ntid))
							{
								model.PricingVerificationList.Add(user);
							}

							break;
						case PtmRole.LOBEstLead:
							model.LOBEstimatingLeadMgrNtid = user.Ntid;
							model.LOBEstimatingLeadMgrDisplayName = user.DisplayName;
							model.IsLOBEstimatingLeadMgrReadOnly = isNewRevision ? false : fullProposalDto.LOBEstimatingLeadSignedDate.HasValue;

							if (!model.LOBEstimatingMgrList.Any(x => x.Ntid == user.Ntid))
							{
								model.LOBEstimatingMgrList.Add(user);
							}

							break;
						case PtmRole.CoverSheetApprover:
							model.CoverSheetApproverNtid = user.Ntid;
							model.CoverSheetApproverDisplayName = user.DisplayName;
							model.IsCoverSheetApproverReadOnly = isNewRevision ? false : fullProposalDto.CoverSheetApproverSignedDate.HasValue;

							if (!model.CoverSheetApproverList.Any(x => x.Ntid == user.Ntid))
							{
								model.CoverSheetApproverList.Add(user);
							}

							break;
						case PtmRole.PeerReviewer:
							model.IndependentReviewerNtid = user.Ntid;
							model.IndependentReviewerDisplayName = user.DisplayName;
							model.IsIndependentReviewerReadOnly = isNewRevision ? false : fullProposalDto.IndependentReviewerSignedDate.HasValue;

							if (!model.IndependentReviewerList.Any(x => x.Ntid == user.Ntid))
							{
								model.IndependentReviewerList.Add(user);
							}

							break;
						default:
							break;
					}
				}

				// If we have started the approval process and the pricing verifier or independent/peer reviewer are not set (not required sometimes), then set them to read-only
				if (fullProposalDto.LeadEstimatorSignedDate.HasValue && !isNewRevision)
				{
					if (!permissions.Any(p => p.Role == PtmRole.PricingVerification))
					{
						model.IsPricingVerificationReadOnly = true;
					}

					if (!permissions.Any(p => p.Role == PtmRole.PeerReviewer))
					{
						model.IsIndependentReviewerReadOnly = true;
					}
				}
			}

			return model;
		}

		/// <summary>
		/// Get view for user general information
		/// </summary>
		/// <param name="proposalId">Proposal Id.  Can be null.</param>
		/// <returns>Proposal User Information Model View</returns>
		public virtual ProposalUserInformationModelView GetDataForProposalUserInformation(int? proposalId, bool isNewRevision)
		{
			ProposalUserInformationModelView model = new ProposalUserInformationModelView();

			model.AdditionalPricingResourceTypeList = EnumUtilities.GetListItemsForEnumSorted(typeof(ResourceType), false);

			FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
			model.IsReadOnly = isNewRevision ? false.ToString().ToLower() : this.IsProposalReadOnly(proposalId, fullProposalDto);

			if (fullProposalDto != null)
			{
				model.UpdateDate = fullProposalDto.UpdateDate;

				ICollection<ProposalPermissionDto> permissions = fullProposalDto.Permissions;

				foreach (ProposalPermissionDto permission in permissions)
				{
					UserDTO user = this.UserMapper.GetById(permission.UserId);
					switch (permission.Role)
					{
						case PtmRole.CaptureManager:
							model.CaptureManagerNtid = user.Ntid;
							model.CaptureManagerDisplayName = user.DisplayName;
							break;
						case PtmRole.CostVolumeLead:
							model.CostVolumeLeadNtid = user.Ntid;
							model.CostVolumeLeadDisplayName = user.DisplayName;
							break;
						case PtmRole.AdditionalPricingResource1:
							model.AdditionalPricingResource1NtId = user.Ntid;
							model.AdditionalPricingResource1DisplayName = user.DisplayName;
							model.AdditionalPricingResource1Type = permission.ResourceType;
							break;
						case PtmRole.AdditionalPricingResource2:
							model.AdditionalPricingResource2NtId = user.Ntid;
							model.AdditionalPricingResource2DisplayName = user.DisplayName;
							model.AdditionalPricingResource2Type = permission.ResourceType;
							break;
						case PtmRole.BackupPricer:
							model.BackupPricerNtId = user.Ntid;
							model.BackupPricerDisplayName = user.DisplayName;
							break;
						case PtmRole.SupplyChainPOCMatl:
							model.SupplyChainPOCMaterialsNtId = user.Ntid;
							model.SupplyChainPOCMaterialsDisplayName = user.DisplayName;
							break;
						case PtmRole.BackupMaterialLead:
							model.SupplyChainPOCMaterialsBackupNtId = user.Ntid;
							model.SupplyChainPOCMaterialsBackupDisplayName = user.DisplayName;
							break;
						case PtmRole.SupplyChainPOCSubs:
							model.SupplyChainPOCSubsNtId = user.Ntid;
							model.SupplyChainPOCSubsDisplayName = user.DisplayName;
							break;
						case PtmRole.BackupSubcontractsLead:
							model.SupplyChainPOCSubsBackupNtId = user.Ntid;
							model.SupplyChainPOCSubsBackupDisplayName = user.DisplayName;
							break;
						case PtmRole.ContractsPOC:
							model.ContractsPOCNtId = user.Ntid;
							model.ContractsPOCDisplayName = user.DisplayName;
							break;
						case PtmRole.BackupContractsPOC:
							model.BackupContractsPOCNtId = user.Ntid;
							model.BackupContractsPOCDisplayName = user.DisplayName;
							break;
						case PtmRole.TechLead:
							model.TechLeadNtid = user.Ntid;
							model.TechLeadDisplayName = user.DisplayName;
							break;
						case PtmRole.ProposalMgr:
							model.ProposalMgrNtid = user.Ntid;
							model.ProposalMgrDisplayName = user.DisplayName;
							break;
						case PtmRole.GenBoeWorkspaceCreator:
							model.GenBoeWorkspaceCreatorNtid = user.Ntid;
							model.GenBoeWorkspaceCreatorDisplayName = user.DisplayName;
							break;
						default:
							break;
					}
				}
			}
			model = PopulateUserInformationLists(model);

			return model;
		}

		public ProposalUserInformationModelView PopulateUserInformationLists(ProposalUserInformationModelView model)
		{
			// Primary and Backup Contracts PoC share the same AD List, get the data once for both
			ICollection<UserDTO> contractsUsers = this.GetUsersForSelectList(PtmRole.ContractsPOC);

			model.ContractLeadList = contractsUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			if (!model.ContractLeadList.Any(x => x.Value == model.ContractsPOCNtId))
			{
				model.ContractLeadList.Insert(0, new SelectListItem() { Value = model.ContractsPOCNtId, Text = model.ContractsPOCDisplayName });
			}
			model.ContractLeadList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select Contracts Lead" });

			model.BackupContractLeadList = contractsUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			if (!model.BackupContractLeadList.Any(x => x.Value == model.BackupContractsPOCNtId))
			{
				model.BackupContractLeadList.Insert(0, new SelectListItem() { Value = model.BackupContractsPOCNtId, Text = model.BackupContractsPOCDisplayName });
			}
			model.BackupContractLeadList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select Backup Contracts Lead" });

			model.GenBoeWorkspaceCreatorList = new Collection<SelectListItem>() { new SelectListItem() { Value = string.Empty, Text = "Select GenBOE Workspace Creator" } };
			ICollection<KeyValuePair<string, string>> workspaceCreatorList = this.genBoePermissionLoader.GetCreateWorkspaceRolesForPtm(model.GenBoeWorkspaceCreatorNtid, model.GenBoeWorkspaceCreatorDisplayName);
			if (workspaceCreatorList.Any())
			{
				model.GenBoeWorkspaceCreatorList.AddRange(workspaceCreatorList.Select(x => new SelectListItem
				{
					Text = x.Value, // Display Name
					Value = x.Key // NTID
				}).ToCollection());
			}

			// Populate Material Leads and backup leads lists
			ICollection<UserDTO> materialsUsers = this.GetUsersForSelectList(PtmRole.SupplyChainPOCMatl);

			model.SupplyChainPOCMaterialsLeadsList = materialsUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			model.SupplyChainPOCMaterialsLeadsList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select PBOE/MPBOE Preparer" });

			model.SupplyChainPOCMaterialsBackupLeadsList = materialsUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			model.SupplyChainPOCMaterialsBackupLeadsList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select Backup PBOE/MPBOE Preparer" });

			// Populate Subcontract Leads and Backup Leads lists
			ICollection<UserDTO> subcontractUsers = this.GetUsersForSelectList(PtmRole.SupplyChainPOCSubs);

			model.SupplyChainPOCSubsLeadsList = subcontractUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			model.SupplyChainPOCSubsLeadsList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select IBOE Preparer" });

			model.SupplyChainPOCSubsBackupLeadsList = subcontractUsers?
				.Select(x => new SelectListItem() { Value = x.Ntid, Text = x.DisplayName })
				.OrderBy(x => x.Text)
				.ToList();
			model.SupplyChainPOCSubsBackupLeadsList.Insert(0, new SelectListItem() { Value = string.Empty, Text = "Select Backup IBOE Preparer" });

			return model;
		}

		/// <summary>
		/// Get the data for the Proposal Comments partial view
		/// </summary>
		/// <param name="proposalId">proposal id</param>
		/// <returns>Modelview for Proposal Comments</returns>
		public ProposalCommentsModelView GetDataForProposalComments(int? proposalId)
		{
			ProposalCommentsModelView model = new ProposalCommentsModelView();

			if (proposalId.HasValue)
			{
				ProposalDto fullProposalDto = this.GetByProposalId(proposalId.Value);

				model.Comments = fullProposalDto?.ProposalSetupComments;
			}

			return model;
		}

		/// <summary>
		/// Validate the Proposal General Information values which can't be done in the ProposlGeneralInformationModelView
		/// </summary>
		/// <param name="proposalGeneralInfo">ProposalGeneralInformationModelView instance</param>
		/// <param name="inValidationErrors">list of validation errors to append to</param>
		/// <param name="isForecasted">Indicates whether this proposal is forecasted or not excluding some validation.</param>
		/// <param name="proposalClass">Proposal Class</param>
		public void ValidateGeneralInfoTypes(ProposalGeneralInformationModelView proposalGeneralInfo, ICollection<ValidationMessage> inValidationErrors, bool isForecasted, int proposalClass)
		{
			if (proposalGeneralInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalGeneralInfo));
			}

			if (inValidationErrors == null)
			{
				throw new ArgumentNullException(nameof(inValidationErrors));
			}

			if (!isForecasted)
			{
				if (proposalGeneralInfo.PricingTool == PricingTool.NotSet)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PRICING_TOOL_REQUIRED));
				}
				else if (proposalGeneralInfo.PricingTool == PricingTool.Other &&
					(string.IsNullOrEmpty(proposalGeneralInfo.PricingToolName) || !regexFreeText.IsMatch(proposalGeneralInfo.PricingToolName)))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PRICING_TOOL_FORMAT_ERROR));

				}

				if (proposalGeneralInfo.BOETool == BOETool.NotSet)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.BOE_TOOL_REQUIRED));
				}
				else if (proposalGeneralInfo.BOETool == BOETool.Other &&
					(string.IsNullOrEmpty(proposalGeneralInfo.BOEToolName) || !regexFreeText.IsMatch(proposalGeneralInfo.BOEToolName)))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.BOE_TOOL_FORMAT_ERROR));

				}

				if (proposalGeneralInfo.CostVolumeTool == CostVolumeTool.NotSet)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.COST_VOLUME_TOOL_REQUIRED));
				}
				else if (proposalGeneralInfo.CostVolumeTool == CostVolumeTool.Other &&
					(string.IsNullOrEmpty(proposalGeneralInfo.CostVolumeToolName) || !regexFreeText.IsMatch(proposalGeneralInfo.CostVolumeToolName)))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.COST_VOLUME_TOOL_FORMAT_ERROR));
				}
			}

			if (!proposalGeneralInfo.IsCCPDRequired.HasValue)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CERTIFIED_COST_PRICING_DATA_REQUIRED));
			}
			else if (!proposalGeneralInfo.IsCCPDRequired.Value && (!proposalGeneralInfo.CcopdNoReason.HasValue || proposalGeneralInfo.CcopdNoReason == CcopdOptionalReason.NotSet))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CCOPD_NO_REASON_REQUIRED));
			}
			else if (!proposalGeneralInfo.IsCCPDRequired.Value && proposalGeneralInfo.CcopdNoReason == CcopdOptionalReason.Other && string.IsNullOrEmpty(proposalGeneralInfo.CcopdNoOtherReason))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CCOPD_NO_OTHER_REASON_REQUIRED));
			}
			else if (proposalGeneralInfo.IsCCPDRequired.Value && proposalClass == FIRM_PROPOSAL_CLASS && !proposalGeneralInfo.IsSupportOfUndefinitized.HasValue)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.IS_IN_SUPPORT_OF_DEFINITIZING_UNDEFINITIZED_REQUIRED));
			}

			if (!proposalGeneralInfo.IsCostVolumeClassified.HasValue)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CERTIFIED_COST_PRICING_DATA_CLASSIFIED_REQUIRED));
			}
			else if (proposalGeneralInfo.IsCostVolumeClassified.Value && !proposalGeneralInfo.AdditionalClassification.HasValue)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.ADITIONAL_CLASSIFICATION_REQUIRED));
			}
		}

		/// <summary>
		/// Performs validation for proposal users.  For a new proposal, validate all of the proposals.  For a completed proposal, only validate the new users
		/// by comparing the current users against the users from a retrieved proposal
		/// </summary>
		/// <param name="proposalId">the id is populated if proposal has been saved</param>
		/// <param name="proposalApprovalsInfo">the proposal approvals to be verified</param>
		/// <param name="proposalUserInfo">the proposal users to be verified</param>
		/// <param name="inValidationErrors">validation errors collection</param>
		/// <returns>True if there are invalid users on an update but were not changed, false otherwise.</returns>
		public bool ValidateUserTypes(int? proposalId, ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo, ICollection<ValidationMessage> inValidationErrors)
		{
			if (proposalApprovalsInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalApprovalsInfo));
			}

			if (proposalUserInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalUserInfo));
			}

			if (inValidationErrors == null)
			{
				throw new ArgumentNullException(nameof(inValidationErrors));
			}

			bool validUnchangedUsers = true;

			if (string.IsNullOrWhiteSpace(proposalUserInfo.CaptureManagerNtid))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CAPTURE_MANAGER_REQUIRED));
			}

			if (string.IsNullOrWhiteSpace(proposalUserInfo.CostVolumeLeadNtid))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.COST_VOLUME_REQUIRED));
			}

			bool invalidContractsPOCNtId = string.IsNullOrWhiteSpace(proposalUserInfo.ContractsPOCNtId);
			bool invalidBackupContractsPOCNtId = string.IsNullOrWhiteSpace(proposalUserInfo.BackupContractsPOCNtId);

			if (invalidContractsPOCNtId)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CONTRACTS_POC_REQUIRED));
			}

			if (invalidBackupContractsPOCNtId)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.BACKCUP_CONTRACTS_POC_REQUIRED));
			}

			if (string.IsNullOrWhiteSpace(proposalUserInfo.ProposalMgrNtid))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PROPOSALMGR_REQUIRED));
			}

			if (!invalidContractsPOCNtId && !invalidBackupContractsPOCNtId && (proposalUserInfo.ContractsPOCNtId == proposalUserInfo.BackupContractsPOCNtId))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CONTRACTS_LEAD_AND_BACKUP_CANNOT_BE_IDENTICAL));
			}

			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsNtId)
				&& !string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsBackupNtId)
				&& proposalUserInfo.SupplyChainPOCMaterialsNtId == proposalUserInfo.SupplyChainPOCMaterialsBackupNtId)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.MATERIAL_LEAD_AND_BACKUP_CANNOT_BE_IDENTICAL));
			}

			if (string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsNtId)
				&& !string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsBackupNtId))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.MATERIAL_LEAD_BACKUP_REQUIRES_LEAD));
			}

			if (!string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsNtId)
				&& !string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsBackupNtId)
				&& proposalUserInfo.SupplyChainPOCSubsNtId == proposalUserInfo.SupplyChainPOCSubsBackupNtId)
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SUBCONTRACTS_LEAD_AND_BACKUP_CANNOT_BE_IDENTICAL));
			}

			if (string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsNtId)
				&& !string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsBackupNtId))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SUBCONTRACTS_LEAD_BACKUP_REQUIRES_LEAD));
			}

			// if this is a saved proposal, this will only validate the changed users, else validate all of the users

			ProposalUserInformationModelView savedProposalUsers = this.GetDataForProposalUserInformation(proposalId, false);

			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.CaptureManagerNtid, savedProposalUsers.CaptureManagerNtid, inValidationErrors, ValidationConstants.ProposalValidationConstants.CAPTURE_MANAGER_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.CostVolumeLeadNtid, savedProposalUsers.CostVolumeLeadNtid, inValidationErrors, ValidationConstants.ProposalValidationConstants.COST_VOLUME_LEAD_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.AdditionalPricingResource1NtId, savedProposalUsers.AdditionalPricingResource1NtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.ADDITIONAL_PRICING_RESOURCE_1_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.AdditionalPricingResource2NtId, savedProposalUsers.AdditionalPricingResource2NtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.ADDITIONAL_PRICING_RESOURCE_2_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.SupplyChainPOCMaterialsNtId, savedProposalUsers.SupplyChainPOCMaterialsNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_POC_MATL_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.SupplyChainPOCMaterialsBackupNtId, savedProposalUsers.SupplyChainPOCMaterialsBackupNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_POC_MATL_BACKUP_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.SupplyChainPOCSubsNtId, savedProposalUsers.SupplyChainPOCSubsNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_POC_SUBS_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.SupplyChainPOCSubsBackupNtId, savedProposalUsers.SupplyChainPOCSubsBackupNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_POC_SUBS_BACKUP_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.ContractsPOCNtId, savedProposalUsers.ContractsPOCNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.CONTRACTS_POC_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.BackupContractsPOCNtId, savedProposalUsers.BackupContractsPOCNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.BACKUP_CONTRACTS_POC_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.BackupPricerNtId, savedProposalUsers.BackupPricerNtId, inValidationErrors, ValidationConstants.ProposalValidationConstants.BACKUP_PRICER_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.ProposalMgrNtid, savedProposalUsers.ProposalMgrNtid, inValidationErrors, ValidationConstants.ProposalValidationConstants.PROPOSAL_MANAGER_INVALID_NTID, false, false) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.TechLeadNtid, savedProposalUsers.TechLeadNtid, inValidationErrors, ValidationConstants.ProposalValidationConstants.TECH_LEAD_INVALID_NTID, true, true) && validUnchangedUsers;
			validUnchangedUsers = this.ValidateUserType(proposalUserInfo.GenBoeWorkspaceCreatorNtid, savedProposalUsers.GenBoeWorkspaceCreatorNtid, inValidationErrors, ValidationConstants.ProposalValidationConstants.WORKSPACE_CREATOR_INVALID_NTID, false, false) && validUnchangedUsers;

			return !validUnchangedUsers;
		}

		/// <summary>
		/// Validate the user type helper method for ValidateUserTypes
		/// </summary>
		/// <param name="proposalUser">the Nt user id</param>
		/// <param name="originalUser">The original proposal NT user id</param>
		/// <param name="inValidationErrors">a collection of error messages</param>
		/// <param name="errorMessage">the error message to add if the user is invalid</param>
		/// <param name="requiredUsPerson">Is the user required to be a US person</param>
		/// <param name="requiredLmEmployee">Is the user required to be an LM employee</param>
		/// <param name="isValidWhenNtidIsNull">What to do when a value is null</param>
		/// <returns>True if the user is valid, otherwise false.</returns>
		private bool ValidateUserType(string proposalUser, string originalUser, ICollection<ValidationMessage> inValidationErrors, string errorMessage, bool requiredUsPerson, bool requiredLmEmployee, bool isValidWhenNtidIsNull = false)
		{
			bool isValid = true;
			if (!string.IsNullOrEmpty(proposalUser))
			{
				if (proposalUser != originalUser)
				{
					// we do full validation if the user is different
					if (!this.validationMethods.IsUserTypeValid(proposalUser, IES.Common.UserType.User, isValidWhenNtidIsNull, requiredUsPerson, requiredLmEmployee))
					{
						isValid = false;
					}
				}
				else
				{
					// if the users are the same, we only validate the following:
					// validation fails if the user is still an active user AND requiredUsPerson or requiredLmEmployee check fails
					isValid = this.validationMethods.IsValidLMandUsEmployeeProperties(originalUser, requiredUsPerson, requiredLmEmployee) != false;
				}

				if (!isValid)
				{
					inValidationErrors.Add(new ValidationMessage(errorMessage));
				}
			}

			return isValid;
		}

		/// <summary>
		/// Validates the proposal
		/// </summary>
		/// <param name="proposalInfo">Proposal information model view</param>
		/// <param name="proposalGeneralInfo">Proposal general information model view</param>
		/// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
		/// <param name="proposalUserInfo">Proposal user information model view</param>
		/// <param name="inValidationErrors">validation error collection</param>
		/// <param name="isForecasted">Whether the proposal Class is Forecasted. If true, then exclude some validation.</param>
		public void ValidateProposal(ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
			ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo,
				ICollection<ValidationMessage> inValidationErrors, bool isForecasted)
		{
			if (proposalInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalInfo));
			}

			if (proposalGeneralInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalGeneralInfo));
			}

			if (proposalApprovalsInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalApprovalsInfo));
			}

			if (proposalUserInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalUserInfo));
			}

			if (inValidationErrors == null)
			{
				throw new ArgumentNullException(nameof(inValidationErrors));
			}

			if (isForecasted)
			{
				if (proposalGeneralInfo.ProposalID > 0)
				{
					// If this is an edit, check to make sure there are no BOEs or RDSB linked to this proposal
					ProposalDto proposal = this.ProposalLoader.GetById(proposalGeneralInfo.ProposalID);
					if (proposal.DocumentId.HasValue)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.INVALID_PROPOSAL_CLASS_FORECAST_DOCUMENT));
					}

					if (!string.IsNullOrWhiteSpace(proposal.TrackingNumber) && this.workspaceDTODataLoader.GetAllWsNamesForTrackingNumber(proposal.TrackingNumber).Any())
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.INVALID_PROPOSAL_CLASS_FORECAST_BOE));
					}
				}
			}
			else
			{
				if (proposalInfo.ContractActionType.Equals(ContractActionType.Other) && string.IsNullOrWhiteSpace(proposalInfo.ContractActionTypeOtherText))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PROPOSAL_CONTRACT_ACTION_TYPE_OTHER_TEXT_REQUIRED));
				}

				if (!proposalInfo.RequestType.HasValue)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.TYPE_OF_REQUEST_REQUIRED));
				}

				if (!string.IsNullOrEmpty(proposalInfo.RFPIssuedDate))
				{
					try
					{
						proposalInfo.RFPIssuedDate.ToDateTime("MM/dd/yyyy");
					}
					catch (FormatException)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.RFP_ISSUED_DATE_FORMAT));
					}
				}

				if (!string.IsNullOrEmpty(proposalInfo.RFPReceivedDate))
				{
					try
					{
						proposalInfo.RFPReceivedDate.ToDateTime("MM/dd/yyyy");
					}
					catch (FormatException)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.RFP_RECEIVED_DATE_FORMAT));
					}
				}

				if (!string.IsNullOrEmpty(proposalInfo.RevisedSubmittalDate))
				{
					try
					{
						proposalInfo.RevisedSubmittalDate.ToDateTime("MM/dd/yyyy");
					}
					catch (FormatException)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.REVISED_SUBMITTAL_DATE_FORMAT));
					}
				}

				if (!proposalInfo.CostElements.Any())
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.ELEMENTS_OF_COST_REQUIRED));
				}

				if (proposalInfo.CostElements.Contains((int)CostElementType.Materials) &&
					string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCMaterialsNtId))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_MATERIAL_REQUIRED));
				}

				if (proposalInfo.CostElements.Contains((int)CostElementType.Subs) &&
					 string.IsNullOrEmpty(proposalUserInfo.SupplyChainPOCSubsNtId))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SUPPLY_CHAIN_SUBS_REQUIRED));
				}

				if (string.IsNullOrWhiteSpace(proposalApprovalsInfo.LeadEstimatorNtid))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_REQUIRED));
				}

				if (proposalUserInfo.AdditionalPricingResource1NtId != null &&
				proposalUserInfo.AdditionalPricingResource1Type == ResourceType.NotSet)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.ADDITIONAL_RESOURCE_1_TYPE_REQUIRED));
				}

				if (proposalUserInfo.AdditionalPricingResource2NtId != null &&
					proposalUserInfo.AdditionalPricingResource2Type == ResourceType.NotSet)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.ADDITIONAL_RESOURCE_2_TYPE_REQUIRED));
				}

				if (string.IsNullOrEmpty(proposalApprovalsInfo.IndependentReviewerNtid))
				{
					// if CCPD Required is set to Yes (or is not set)
					if (!proposalGeneralInfo.IsCCPDRequired.HasValue || proposalGeneralInfo.IsCCPDRequired.Value == true)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.INDEPENDENT_REVIEWER_REQUIRED));
					}
				}

				if (string.IsNullOrEmpty(proposalApprovalsInfo.PricingVerificationNtid))
				{
					// if CCPD Required is set to Yes or is not set, then this is required
					if (!proposalGeneralInfo.IsCCPDRequired.HasValue || proposalGeneralInfo.IsCCPDRequired.Value == true)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PRICING_VERIFICATION_REQUIRED));
					}
				}

				if (string.IsNullOrEmpty(proposalApprovalsInfo.CoverSheetApproverNtid) && (!proposalGeneralInfo.IsCCPDRequired.HasValue || proposalGeneralInfo.IsCCPDRequired.Value == true))
				{
					// coversheet approver is required if CCPDRequired is set to Yes or is not set
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.COVER_SHEET_APPROVER_REQUIRED));
				}

				// The Lead Estimator and Independent Reviewer cannot be the same person
				// It's unnecessary to check for this constraint if either value is null
				if (proposalApprovalsInfo.LeadEstimatorNtid != null && proposalApprovalsInfo.IndependentReviewerNtid != null)
				{
					if (proposalApprovalsInfo.LeadEstimatorNtid == proposalApprovalsInfo.IndependentReviewerNtid)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON));
					}
				}

				// The Lead Estimator and Pricing Verification cannot be the same person
				// It's unnecessary to check for this constraint if either value is null
				if (proposalApprovalsInfo.LeadEstimatorNtid != null && proposalApprovalsInfo.PricingVerificationNtid != null)
				{
					if (proposalApprovalsInfo.LeadEstimatorNtid == proposalApprovalsInfo.PricingVerificationNtid)
					{
						inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_PRICING_VERIFICATION_CANNOT_BE_SAME_PERSON));
					}
				}

				// The Lead Estimator and LOB Lead cannot be the same person; it's unnecessary to check for this constraint if either value is null
				if (!string.IsNullOrEmpty(proposalApprovalsInfo.LeadEstimatorNtid) && !string.IsNullOrEmpty(proposalApprovalsInfo.LOBEstimatingLeadMgrNtid)
						&& proposalApprovalsInfo.LeadEstimatorNtid == proposalApprovalsInfo.LOBEstimatingLeadMgrNtid)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_LOB_LEAD_CANNOT_BE_SAME_PERSON));
				}

				if (string.IsNullOrWhiteSpace(proposalInfo.RFPNumber))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.RFP_NUMBER_REQUIRED));
				}

				if (string.IsNullOrWhiteSpace(proposalInfo.RFPIssuedDate))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.RFP_ISSUED_DATE_REQUIRED));
				}

				if (string.IsNullOrWhiteSpace(proposalInfo.RFPReceivedDate))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.RFP_RECEIVED_DATE_REQUIRED));
				}

				if (!proposalInfo.IsScheduleProposal.HasValue)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SCHEDULE_PROPOSAL_REQUIRED));
				}
			}

			if (!string.IsNullOrEmpty(proposalInfo.AnticipatedDeliveryDate))
			{
				try
				{
					DateTime anticipatedDelivery = proposalInfo.AnticipatedDeliveryDate.ToDateTime("MM/dd/yyyy");
					if (isForecasted)
					{
						int forecastedDaysOut = IES.Common.ConfigurationUtilities.GetAppSetting<int>("ForecastedDaysOut");
						if (anticipatedDelivery < DateTime.Now.AddDays(forecastedDaysOut))
						{
							inValidationErrors.Add(new ValidationMessage(string.Format(ValidationConstants.ProposalValidationConstants.ANTICIPATED_DELIVERY_DATE_INVALID_FORECAST, forecastedDaysOut)));
						}
					}
				}
				catch (FormatException)
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.ANTICIPATED_DELIVERY_DATE_FORMAT));
				}
			}

			if (proposalInfo.ProposalTitle != null)
			{
				if (!this.ProposalLoader.IsProposalTitleUnique(proposalInfo.ProposalID, proposalInfo.ProposalTitle))
				{
					inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.PROPOSAL_TITLE_MUST_BE_UNIQUE));
				}
			}

			// This is to catch the error in new proposals where a selection isn't made
			if (proposalInfo.ISGSRole == ISGSRole.NotSet && !inValidationErrors.Any(x => x.ValidationIssue == ValidationConstants.ProposalValidationConstants.SSC_ROLE_REQUIRED))
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.SSC_ROLE_REQUIRED));
			}

			if (!proposalInfo.ContractType.Any())
			{
				inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.CONTRACT_TYPE_REQUIRED));
			}
		}

		/// <summary>
		/// Validate that new revision doesn't already exist
		/// </summary>
		/// <param name="proposalInfo">Proposal Info</param>
		public void ValidateNewRevisionDoesNotExist(ProposalInformationModelView proposalInfo)
		{
			if (proposalInfo == null)
			{
				throw new ArgumentNullException(nameof(proposalInfo));
			}

			if (this.ProposalLoader.GetIdByTrackingNumber(proposalInfo.ProposalTrackingNumber) > 0)
			{
				ValidationMessage validationError = new ValidationMessage(ValidationConstants.ProposalRevisionConstants.REVISION_ALREADY_EXISTS);
				validationError.FormIDToTarget = GenTRAC.ActionLogic.ProposalControllerLogic.PROPOSAL_INFO_FORM;

				// Throw error now - won't ever be able to save this revision, so no need to validate anything else
				throw new ValidationException(new Collection<ValidationMessage>() { validationError });
			}
		}

		/// <summary>
		/// Get the active user.
		/// </summary>
		/// <returns>Current active user</returns>
		public UserDTO GetActiveUser()
		{
			return this.UserMapper.GetActiveUser();
		}

		/// <summary>
		/// Return available ProgramAreas for selected line of business
		/// </summary>
		/// <param name="lineOfBusinessId">Line of Business to filter</param>
		/// <param name="programAreaId">Optional id for the selected Program Area.</param>
		/// <returns>Program Area</returns>
		public string GetProgramAreasForLineOfBusiness(int? lineOfBusinessId, int? programAreaId)
		{
			return this.orgStructureDataMapper.GetProgramAreaHtmlOptionsForLineOfBusiness(lineOfBusinessId, programAreaId);
		}

		/// <summary>
		/// Return available Contract Types for selected Contract Type Group
		/// </summary>
		/// <param name="contractTypeGroup">Contract Type Group to filter</param>
		/// <param name="includeIDIQ">Whether to include IDIQ options.</param>
		/// <returns>Contract Types</returns>
		public string GetContractTypesForContractTypeGroup(string contractTypeGroup, bool includeIDIQ)
		{
			StringBuilder selectList = new StringBuilder();

			if (!string.IsNullOrEmpty(contractTypeGroup))
			{
				int group = int.Parse(contractTypeGroup);

				foreach (PickListDto contractType in this.pickListMapper.GetChildren(PickListEnum.ContractType, group).Where(x => x.IsActive))
				{
					if (contractType.Text != Constants.IDIQ_CONTRACT_TYPE || includeIDIQ)
					{
						selectList.Append(string.Format("<option value=\"{0}\">{1}</option>",
							contractType.Id, contractType.Text));
					}
				}
			}

			return selectList.ToString();
		}

		/// <summary>
		/// Determines whether proposal is read only based on proposal state and current user
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <param name="fullProposal">Full proposal</param>
		/// <returns>"true" if readonly, "false" if editable</returns>
		public string IsProposalReadOnly(int? proposalId, ProposalDto fullProposal)
		{
			bool readOnly = false;

			if (fullProposal != null && fullProposal.ProposalStatus != ProposalStatus.InProgress && fullProposal.ProposalStatus != ProposalStatus.Completed
				&& fullProposal.ProposalStatus != ProposalStatus.PendingCertification && fullProposal.ProposalStatus != ProposalStatus.PendingAward)
			{
				// proposal status is Archived, Deleted, or Revision - always read only
				readOnly = true;
			}
			else
			{
				// check permission of current user
				SecurityAuthorizationAndRole authorization = this.CheckPermissions(PtmSecurityPage.Proposal, proposalId);
				if (authorization.Authorization == SecurityAuthorization.Read)
				{
					readOnly = true;
				}
			}

			return readOnly.ToString().ToLower();
		}

		/// <summary>
		/// Determines whether certification of proposal is read only based on proposal state and current user
		/// </summary>
		/// <param name="proposalId">The proposal Id.</param>
		/// <param name="proposalStatus">Proposal Status</param>
		/// <param name="markedAsCertificationNotRequired">Certification is marked as not-required</param>
		/// <returns>"true" if readonly, "false" if editable</returns>
		public string IsCertificationReadOnly(int proposalId, ProposalStatus proposalStatus, bool markedAsCertificationNotRequired)
		{
			bool readOnly = proposalStatus == ProposalStatus.Revised || markedAsCertificationNotRequired;

			if (!readOnly)
			{
				// check permission of current user
				SecurityAuthorizationAndRole authorization = this.CheckPermissions(PtmSecurityPage.CertificationTimeline, proposalId);
				if (authorization.Authorization == SecurityAuthorization.Read)
				{
					readOnly = true;
				}
			}

			return readOnly.ToString().ToLower();
		}

		/// <summary>
		/// This method will figure out if the checklist type is changing becuase of a save.
		/// </summary>
		/// <param name="proposalId">The proposal id.</param>
		/// <param name="newProposalChecklistType">The current saved checklist type.</param>
		/// <returns>True if the checklist type changed, false otherwise.</returns>
		private bool DidChecklistTypeChange(int proposalId, ProposalChecklistType newProposalChecklistType)
		{
			bool updateChecklistType = false;

			FullProposal fullProposal = this.GetFullProposalDto(proposalId);
			if (fullProposal != null)
			{
				ChecklistContentDto parData = fullProposal.ProposalChecklistPARData;

				if (ProposalChecklistType.InternationalCommercial.IsActive() &&
					(parData.ChecklistType == ProposalChecklistType.InternationalCommercial && newProposalChecklistType != ProposalChecklistType.InternationalCommercial ||
					(parData.ChecklistType != ProposalChecklistType.InternationalCommercial && parData.ChecklistType != ProposalChecklistType.NotSet) && newProposalChecklistType == ProposalChecklistType.InternationalCommercial))
				{
					updateChecklistType = true;
				}
			}

			return updateChecklistType;
		}

		/// <summary>
		/// Sends the email to the LOB Estimating Manager/Delegate when a user selects a pricing tool other than ProPricer and/or a boe tool other than genBoe
		/// </summary>
		/// <param name="lobEstMgrDelNtid">The LOB Estimating Manager/Delegate ntid</param>
		/// <param name="proposalInfo">The proposal info</param>
		public void SendNonpreferredToolsEmail(string lobEstMgrDelNtid, ProposalInformationModelView proposalInfo)
		{
			UserDTO lobEstMgrDel = this.userLoader.GetByNtid(lobEstMgrDelNtid);
			this.emailer.SendNonpreferredToolsEmail(this.securityInformation.ActiveUserData, lobEstMgrDel, proposalInfo);
		}

		/// <summary>
		/// Sends the email to the LOB Estimating Manager/Delegate when a user adjusts a previously non-preferred a pricing and/or a boe tool to ProPricer and genBoe
		/// </summary>
		/// <param name="lobEstMgrDelNtid">The LOB Estimating Manager/Delegate ntid</param>
		/// <param name="proposalInfo">The proposal info</param>
		public void SendPreferredToolsEmail(string lobEstMgrDelNtid, ProposalInformationModelView proposalInfo)
		{
			UserDTO lobEstMgrDel = this.userLoader.GetByNtid(lobEstMgrDelNtid);
			this.emailer.SendPreferredToolsEmail(this.securityInformation.ActiveUserData, lobEstMgrDel, proposalInfo);
		}

		#region Proposal Revisions

		/// <summary>
		/// Validate the Proposal before creating a new Revision
		/// </summary>
		/// <param name="proposalId">Proposal to validate</param>
		public void ValidateSaveNewRevision(int proposalId)
		{
			ProposalDto proposal = this.GetByProposalId(proposalId);

			ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

			if (proposal.WorkflowStatus != WorkflowStatus.ProposalLocked)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.WORKFLOW_NOT_COMPLETED));
			}

			if (proposal.ProposalStatus == ProposalStatus.Completed && proposal.CertificationTimelineCompleted.HasValue)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.CERT_TIMELINE_COMPLETE));
			}

			if (proposal.ProposalStatus == ProposalStatus.Revised)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NOT_LATEST_VERSION_NEW_REVISION));
			}

			if (!this.IsCurrentUserPricerOrBackupOrSysAdmin(proposal.Id))
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NOT_PERMITTED_NEW_REVISION));
			}

			if (validationErrors.Any())
			{
				throw new ValidationException(validationErrors);
			}
		}

		/// <summary>
		/// Set the given proposal to the Revised Status when creating a new Revision
		/// </summary>
		/// <param name="proposalId">Proposal ID</param>
		/// <param name="proposalUpdateDate">Proposal DTO Update Date</param>
		public void SetProposalRevised(int proposalId, DateTime proposalUpdateDate)
		{
			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.SetProposalRevised", this.log))
			{
				this.ProposalLoader.UpdateProposalStatus(proposalId, proposalUpdateDate, ProposalStatus.Revised);
			}
		}

		/// <summary>
		/// Revert a Revised Proposal to its previous state - Submitted for CCoPD "Yes", Completed for CCoPD "No"
		/// </summary>
		/// <param name="proposalId">ID of Revised Proposal</param>
		public void RevertRevisedProposal(int proposalId)
		{
			ProposalDto proposal = this.ProposalLoader.GetById(proposalId);

			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.RevertRevisedProposal", this.log))
			{
				if (proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value)
				{
					this.ProposalLoader.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.PendingCertification);
				}
				else
				{
					this.ProposalLoader.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.PendingAward);
				}
			}
		}

		/// <summary>
		/// Validate that a Proposal is able to be reverted to the prior version
		/// </summary>
		/// <param name="proposal">Proposal being reverted</param>
		public void ValidateRevertRevisionToPriorVersion(ProposalDto proposal)
		{
			ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

			if (proposal.RevisionOfId == null)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NO_PRIOR_VERSION));
			}

			if (this.ProposalLoader.GetAllSlim().Any(x => x.RevisionOfId == proposal.Id))
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NOT_LATEST_VERSION_PRIOR_VERSION));
			}

			if (proposal.ProposalStatus != ProposalStatus.InProgress)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NOT_IN_PROGRESS));
			}

			if (!this.IsCurrentUserPricerOrBackupOrSysAdmin(proposal.Id))
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.NOT_PERMITTED_PRIOR_VERSION));
			}

			if (proposal.DocumentId.HasValue)
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.CANNOT_HAVE_DOCUMENT));
			}

			if (this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo().Any(x => x.TrackingNumber == proposal.TrackingNumber))
			{
				validationErrors.Add(new ValidationMessage(ValidationConstants.ProposalRevisionConstants.CANNOT_HAVE_WORKSPACE));
			}

			if (validationErrors.Any())
			{
				throw new ValidationException(validationErrors);
			}
		}

		/// <summary>
		/// Get the new tracking number for the Revision
		/// Tracking number will be appended with "-PRx" where x is the number of the revision
		/// </summary>
		/// <param name="currentTrackingNumber">Current Tracking Number</param>
		/// <param name="newRevisionSuffix">the new Revision suffix</param>
		/// <returns>New tracking number for the revision</returns>
		private string GetRevisionTrackingNumber(string currentTrackingNumber, out string newRevisionSuffix)
		{
			// Get base tracking number without any revision suffix
			int suffixIndex = currentTrackingNumber.IndexOf(REVISION_SUFFIX);
			string baseTrackingNumber = currentTrackingNumber;
			if (suffixIndex > 0)
			{
				baseTrackingNumber = currentTrackingNumber.Substring(0, suffixIndex);
			}

			ICollection<ProposalDto> trackingNumberData = this.ProposalLoader.GetAllSlim().Where(x => x.TrackingNumber != null &&
				(x.TrackingNumber == baseTrackingNumber || x.TrackingNumber.StartsWith(baseTrackingNumber))).ToCollection();
			if (trackingNumberData.Count() > 1)
			{
				// Second or later revision
				// extract the revision numbers
				ICollection<string> revisionNumberStrings = trackingNumberData.Where(x => x.TrackingNumber.StartsWith(baseTrackingNumber) && x.TrackingNumber != baseTrackingNumber)
					.Select(x => x.TrackingNumber.Substring(suffixIndex + REVISION_SUFFIX.Length)).ToCollection();
				ICollection<int> revisionNumbers = new Collection<int>();

				// convert numbers to ints
				foreach (string revision in revisionNumberStrings)
				{
					int revisionNumber;
					if (int.TryParse(revision, out revisionNumber))
					{
						revisionNumbers.Add(revisionNumber);
					}
				}

				// get the highest number, or 0 if there are none
				int highestRevision = revisionNumbers.Any() ? revisionNumbers.OrderByDescending(x => x).First() : 0;

				// increment the highest revision number to get the new suffix
				newRevisionSuffix = REVISION_SUFFIX + ++highestRevision;
			}
			else
			{
				// First revision - PR1
				newRevisionSuffix = REVISION_SUFFIX + "1";
			}

			return baseTrackingNumber + newRevisionSuffix;
		}

		/// <summary>
		/// Get a new unique title for the revision by adding the revision suffix, removing any previous suffix
		/// </summary>
		/// <param name="oldTitle">Title of the old Proposal</param>
		/// <param name="revisionSuffix">New Revision Suffix from the new Tracking Number</param>
		/// <returns>the new title</returns>
		private string GetNewRevisionProposalTitle(string oldTitle, string revisionSuffix)
		{
			// Remove any existing suffix from the old title
			string baseTitle = oldTitle;
			int suffixIndex = oldTitle.IndexOf(REVISION_SUFFIX);

			if (suffixIndex > 0)
			{
				baseTitle = oldTitle.Substring(0, suffixIndex);
			}

			// append the new suffix
			string newTitle = baseTitle + revisionSuffix;

			// confirm it's unique
			// note: shouldn't happen very often that it's not unique unless a user edits the title to the title of a future revision
			bool titleUnique;
			int additionalSuffix = 0;

			do
			{
				if (!this.ProposalLoader.IsProposalTitleUnique(-1, newTitle))
				{
					// if not unique, append a number to the end, incrementing until unique
					titleUnique = false;
					newTitle = baseTitle + revisionSuffix + "_" + additionalSuffix++;
				}
				else
				{
					titleUnique = true;
				}
			}
			while (!titleUnique);

			return newTitle;
		}

		/// <summary>
		/// Gets Revision History for the specific proposal
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Revision History</returns>
		public ICollection<RevisionHistoryModelView> GetRevisionHistory(int proposalId)
		{
			ICollection<RevisionHistoryModelView> result = this.ProposalLoader.GetRevisionHistory(proposalId);

			foreach (RevisionHistoryModelView prop in result)
			{
				prop.DisplayProposalSetupTab &= this.CheckPermissions(PtmSecurityPage.Proposal, prop.ProposalId).Authorization != SecurityAuthorization.None;
				prop.DisplayChecklistTab &= this.CheckPermissions(PtmSecurityPage.Checklist, prop.ProposalId).Authorization != SecurityAuthorization.None;
				prop.DisplayPSATab &= this.CheckPermissions(PtmSecurityPage.PostSubmittalAttachments, prop.ProposalId).Authorization != SecurityAuthorization.None;
				prop.DisplayApprovalsTab &= this.CheckPermissions(PtmSecurityPage.Approvals, prop.ProposalId).Authorization != SecurityAuthorization.None;
				prop.DisplayCertificationTab &= this.CheckPermissions(PtmSecurityPage.CertificationTimeline, prop.ProposalId).Authorization != SecurityAuthorization.None;
				prop.DisplayRevisionTab &= this.CheckPermissions(PtmSecurityPage.RevisionHistory, prop.ProposalId).Authorization != SecurityAuthorization.None;
			}

			return result;
		}
		#endregion

		#region Certification Timeline Validate / Save

		/// <summary>
		/// Saves the certification timeline.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="model">The model.</param>
		public void SaveCertificationTimeline(int proposalId, ProposalCertificationTimelineModelView model)
		{
			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.SaveCertificationTimeline", this.log))
			{
				this.ValidateCertification(proposalId, model, false);
				ProposalDto proposal = this.ConvertCertificationModelToDto(proposalId, model, false);

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
				{
					this.ProposalMediator.SaveProposal(proposal);
					scope.Complete();
				}
			}
		}

		/// <summary>
		/// Validates the certification timeline.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="model">The model.</param>
		/// <param name="isComplete">if set to <c>true</c> [is validating as complete].</param>
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
		internal void ValidateCertification(int proposalId, ProposalCertificationTimelineModelView model, bool isComplete)
		{
			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}

			ProposalDto proposal = this.ProposalLoader.GetById(proposalId);
			ContractsDto contract = contractsLoader.GetContractForProposal(proposalId);

			if (model.ReasonCertificationNotRequired.HasValue && isComplete)
			{
				throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.COMPLETE_FAILED_PROPOSAL);
			}

			if (model.ReasonCertificationNotRequired.HasValue && proposal.ProposalStatus != ProposalStatus.PendingCertification && proposal.ProposalStatus != ProposalStatus.Completed
				&& proposal.ProposalStatus != ProposalStatus.PendingAward)
			{
				throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.CERTIFICATION_NOT_REQUIRED_WRONG_STATE);
			}

			if (model.ReasonCertificationNotRequired == ReasonCertificationNotRequired.Other && string.IsNullOrEmpty(model.OtherReasonCommentCertification))
			{
				throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.OTHER_REASON_COMMENT_REQUIRED);
			}

			DateTime? agreement = DateTime.TryParse(model.AgreementDate, out DateTime agreementDt) ? (DateTime?)agreementDt : null;
			DateTime? certification = DateTime.TryParse(model.CertificationDate, out DateTime certificationDt) ? (DateTime?)certificationDt : null;
			bool agreementAndCertSet = agreement.HasValue && certification.HasValue;
			TimeSpan daysToCert = agreementAndCertSet ? certification.Value - agreement.Value : new TimeSpan(0);

			if (!model.ReasonCertificationNotRequired.HasValue)
			{
				if (agreementAndCertSet && daysToCert.TotalDays > 5.0)
				{
					// Comments are now required
					if (string.IsNullOrWhiteSpace(model.Comments))
					{
						throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.COMMENTS_REQUIRED);
					}
				}

				if (isComplete)
				{
					// validate that all 3 required fields are set
					if (!agreement.HasValue)
					{
						throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.AGREEMENT_DATE_REQUIRED);
					}

					if (!certification.HasValue)
					{
						throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.CERTIFICATION_DATE_REQUIRED);
					}

					if (!model.CutOffDateUtilization.HasValue)
					{
						throw new ValidationException(ValidationConstants.CertificationTimelineValidationConstants.CUTOFF_DATE_UTILIZATION_REQUIRED);
					}
				}
			}

			if (contract.NegotiationsSubmitted < agreement)
			{
				throw new ValidationException(Constants.INVALID_NEGOTIATIONS_SUBMITTED);
			}

			if (daysToCert.TotalDays < 0)
			{
				throw new ValidationException(Constants.INVALID_DAYS_TO_CERT);
			}
		}

		/// <summary>
		/// Converts the page viewmodel into the Dto
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <param name="model">Certification Model View</param>
		/// <param name="isCertificationComplete">Are we completing the proposal</param>
		/// <returns>Proposal DTO</returns>
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
		internal ProposalDto ConvertCertificationModelToDto(int proposalId, ProposalCertificationTimelineModelView model, bool isCertificationComplete)
		{
			ProposalDto proposal = this.ProposalLoader.GetById(proposalId);
			proposal.Updateable = UpdateType.Upsert;

			if (model.ReasonCertificationNotRequired.HasValue)
			{
				isCertificationComplete = true;
				proposal.ReasonCertificationNotRequired = model.ReasonCertificationNotRequired;
				proposal.OtherReasonComment = model.OtherReasonCommentCertification;

				proposal.AgreementDate = null;
				proposal.CertificationDate = null;
				proposal.CutOffDateUtilization = null;
				proposal.Comments = null;
			}
			else
			{
				// Marking the proposal required (when it was not required before) resets the flow
				if (proposal.ReasonCertificationNotRequired.HasValue)
				{
					proposal.ProposalStatus = ProposalStatus.PendingCertification;
					proposal.CertificationTimelineCompleted = null;
				}

				proposal.AgreementDate = DateTime.TryParse(model.AgreementDate, out DateTime agreementDate) ? (DateTime?)agreementDate : null;
				proposal.CertificationDate = DateTime.TryParse(model.CertificationDate, out DateTime certificationDate) ? (DateTime?)certificationDate : null;

				proposal.CutOffDateUtilization = model.CutOffDateUtilization;
				proposal.Comments = model.Comments;

				proposal.ReasonCertificationNotRequired = null;
				proposal.OtherReasonComment = null;
			}

			if (isCertificationComplete)
			{
				proposal.ProposalStatus = ProposalStatus.PendingAward;
				proposal.CertificationTimelineCompleted = DateTime.Now;
			}

			return proposal;
		}

		/// <summary>
		/// Completes the certification timeline.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="model">The model.</param>
		public void CompleteCertificationTimeline(int proposalId, ProposalCertificationTimelineModelView model)
		{
			using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ProposalControllerLogic.CompleteCertificationTimeline", this.log))
			{
				this.ValidateCertification(proposalId, model, true);
				ProposalDto proposal = this.ConvertCertificationModelToDto(proposalId, model, true);

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
				{
					this.ProposalMediator.SaveProposal(proposal);
					scope.Complete();
				}
			}
		}

		#endregion
	}
}
