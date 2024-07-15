// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.Entity.Infrastructure;
	using System.Linq;
	using System.Web.Mvc;
	using GenTRAC.DataBridge.DTO.Permission;
	using GenTRAC.Models;
	using IES.Common;

	/// <summary>
	/// Proposal dto data loader
	/// </summary>
	public class ProposalLoader : DataLoader<ProposalDto>, IProposalLoader
	{
		/// <summary>
		/// The next generation number
		/// </summary>
		private static int? nextGenerationNumber;

		/// <summary>
		/// The next generation number lock
		/// </summary>
		private static object nextGenerationNumberLock = new object();

		/// <summary>
		/// The forecast generation number format.
		/// </summary>
		private const string FORECAST_NUMBER_FORMAT = "F-{0}-";

		/// <summary>
		/// Gets the next generation number.
		/// </summary>
		private static int NextGenerationNumber
		{
			get
			{
				int nextNumber;
				lock (nextGenerationNumberLock)
				{
					DateTime day = DateTime.Now;
					// if we don't have the next generation number cached or if this is the start of a new year retrieve the next number from the DB
					if (nextGenerationNumber == null || (day.Month == 1 && day.Day == 1))
					{
						nextGenerationNumber = RetrieveNextGenerationNumber();
					}

					nextNumber = nextGenerationNumber.Value;
					nextGenerationNumber++;
				}

				return nextNumber;
			}
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ProposalLoader()
		{
			this.Log = new Logger(typeof(ProposalLoader));
		}

		/// <summary>
		/// Returns all Proposal IDs from the DB
		/// </summary>
		/// <returns>Proposal Ids</returns>
		[DbQuery]
		public ICollection<int> GetAllIds()
		{
			ICollection<int> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetAllIds", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					// select the Proposal IDs from the database
					IQueryable<int> resultLinq = from x in dbModel.Proposals
												 select x.ProposalID;

					toReturn = resultLinq.ToArray();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Returns a collection of proposal DTO by many IDs
		/// </summary>
		/// <param name="ids">Proposal Id</param>
		/// <returns>The ProposalDto, null if none found</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery]
		public override ICollection<ProposalDto> GetByIds(ICollection<int> ids)
		{
			ICollection<ProposalDto> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetByIds", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					// select the Proposal IDs from the database
					// eagerly load ContractTypeLUs and CostElementLUs and ProposalClassLU for later use
					toReturn = dbModel.Proposals.Include("ContractTypeLUs").Include("CostElementLUs").Include("ProposalClassLU")
						.Where(x => ids.Contains(x.ProposalID))
						.Select(entity => new
						{
							Id = entity.ProposalID,
							TrackingNumber = entity.ProposalTrackingID,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = (ProposalStatus)entity.ProposalStatusID,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalTypeID,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = (ISGSRole)entity.ISGSRoleID,
							Request = entity.RequestTypeID,
							ProposalClass = entity.ProposalClassID,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaID,
							ProposalLocation = (ProposalLocation)entity.ProposalLocationID,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = (PricingTool)entity.PricingToolID,
							PricingToolName = entity.PricingToolName,
							BoeTool = (BOETool)entity.BOEToolID,
							BoeToolName = entity.BOEToolName,
							DeliveryDate = entity.AnticipatedDeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = (CustomerType)entity.CustomerTypeID,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroupID.HasValue ? entity.ContractTypeGroupID.Value : 0, // 0 is Not Set
							CreatedByUserId = entity.CreatedByUserID,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
							WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDT,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDT,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDT,
							PricingVerifierSignatureComment = entity.PricingVerifierSignComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDT,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDT,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.CCPDRequired,
							IsCostVolumeClassified = entity.CostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingID,
							IsForecastProposal = entity.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
							CostElementTypeIds = entity.CostElementLUs.Select(x => x.CostElementID),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							entity.ReasonCertificationNotRequired,
							entity.OtherReasonComment,
							ProposalSetupComments = entity.SetupComments,
							InformationComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ProposalCompletedDate = entity.ProposalCompletedDate,
							ContractActionType = entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = (CostVolumeTool)entity.CostVolumeToolID,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ProposalCompletedDate = entity.ProposalCompletedDate,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Returns the Proposal ID for a Proposal given its Proposal Tracking Number.
		/// </summary>
		/// <param name="inTrackingNumber">Proposal Tracking Number</param>
		/// <returns>Proposal ID, -1 if none found</returns>
		[DbQuery]
		public int GetIdByTrackingNumber(string inTrackingNumber)
		{
			if (inTrackingNumber == null)
			{
				throw new ArgumentNullException(nameof(inTrackingNumber));
			}

			int toReturn = -1;

			string trimmedTrackingNumber = inTrackingNumber.Trim().ToLower();
			if (trimmedTrackingNumber.StartsWith("f"))
			{
				// this is a Forecast tracking ID
				using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetIdByTrackingNumber", Log))
				{
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						toReturn = (from x in dbModel.Proposals
									where x.ForecastedTrackingID.Trim().ToLower() == trimmedTrackingNumber
									select x.ProposalID).FirstOrDefault();
					}

					toReturn = toReturn == 0 ? -1 : toReturn;
				}
			}
			else
			{
				// Handle both the current and legacy tracking number format
				string altTrackingNumber = string.Empty;
				if (inTrackingNumber.Length == Constants.TRACKING_NUMBER_LENGTH)
				{
					altTrackingNumber = Constants.PROPOSAL_CENTURY + inTrackingNumber;
				}
				else if (inTrackingNumber.Length == Constants.LEGACY_TRACKING_NUMBER_LENGTH)
				{
					altTrackingNumber = inTrackingNumber.Substring(2, Constants.TRACKING_NUMBER_LENGTH);
				}

				altTrackingNumber = altTrackingNumber.Trim().ToLower();

				using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetIdByTrackingNumber", Log))
				{
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						toReturn = (from x in dbModel.Proposals
									where x.ProposalTrackingID.Trim().ToLower() == trimmedTrackingNumber
									select x.ProposalID).FirstOrDefault();

						// Search for alt TrackingNumber only if original query failed
						if (toReturn == 0 && !string.IsNullOrEmpty(altTrackingNumber))
						{
							toReturn = (from x in dbModel.Proposals
										where x.ProposalTrackingID.Trim().ToLower() == altTrackingNumber
										select x.ProposalID).FirstOrDefault();
						}
					}

					toReturn = toReturn == 0 ? -1 : toReturn;
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets a list of Proposal IDs for Proposals that the designated users are allowed to see
		/// </summary>
		/// <param name="userids">List of user ids</param>
		/// <returns>List of Proposal ids</returns>
		[DbQuery]
		public ICollection<int> GetProposalIdsByUser(ICollection<int> userids)
		{
			ICollection<int> toReturn = new List<int>();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalIdsByUser", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					IQueryable<int> proposalIds = dbModel.ProposalUserRoles.Where(u => userids.Contains(u.UserID)).Select(c => c.ProposalID).Distinct();
					toReturn = new List<int>(proposalIds);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets a list of Proposal Data to use for the Home Proposal View, it includes proposals that the passed in user is allowed
		/// to see, along with the proposal information of their reports (down the tree to Individual contributors) where the employee
		/// holds either the Pricer or Cost Volume Lead roles.
		/// If showProposalsForMyOrganization is set, include proposals for the user's organization 
		/// (i.e. Product Lines where their group(s) have the "Viewer" role).
		/// </summary>
		/// <param name="proposalStatus">The status of the proposals to view.</param>
		/// <param name="filterStartDate">The start date for the filter.</param>
		/// <param name="filterEndDate">The end date for the filter.</param>
		/// <param name="searchString">The search string.</param>
		/// <param name="ntID">The users NT ID.</param>
		/// <param name="showProposalsForMyOrganization">True to show proposals for the user's organization; false to show only the user's proposals.</param>
		/// <param name="userAndGroupIDs">XML list of User and Group IDs for the user. Only used when showProposalsForMyOrganization is true.</param>
		/// <param name="proposalClassFilterID">The filter ID for the proposal class of the proposals to view.</param>
		/// <returns>A list of proposals for the given user.</returns>
		public ICollection<HomeProposalViewDto> GetProposalsByUser(int? proposalStatus, DateTime? filterStartDate, DateTime? filterEndDate, string searchString, string ntID, bool showProposalsForMyOrganization, string userAndGroupIDs, int? proposalClassFilterID)
		{
			ICollection<HomeProposalViewDto> toReturn;

			using (new StopwatchTimer("ProposalLoader.GetProposalsByUser", this.Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					dbModel.Database.CommandTimeout = 360;  // extend time for admins

					ICollection<int> proposalIdsWithWriteAccess = GetProposalIdsWithLinkedDocumentWriteAccess(ntID, dbModel);

					toReturn = dbModel.getMyProposals(proposalStatus, filterStartDate, filterEndDate, searchString ?? string.Empty, ntID,
						showProposalsForMyOrganization, userAndGroupIDs, proposalClassFilterID)
						.Select(
							entity => new HomeProposalViewDto
							{
								ProposalId = entity.ProposalID,
								TrackingNumber = entity.Tracking_Number,
								ForecastedTrackingNumber = entity.Forecasted_Tracking_Number,
								ProposalTitle = entity.Proposal_Title,
								Status = ExtensionMethods.GetValueFromDescription<ProposalStatus>(entity.Proposal_Status),
								Customer = entity.Customer,
								ProgramArea = entity.Program_Area,
								EstValue = entity.Estimated_Value,
								SubmittedValue = entity.Submitted_Value,
								CaptureManagerDisplayName = entity.Capture_Manager,
								CostVolumeLeadDisplayName = entity.Cost_Volume_Lead,
								PeerReviewerDisplayName = entity.Peer_Reviewer,
								PricerDisplayName = entity.Pricer_Name,
								ProposalSubmittalDate = entity.Proposal_Submit_Date,
								ChecklistCompleteDate = entity.ChecklistCompleteDate,
								ProposalDateAssigned = entity.Date_Assigned,
								ProposalDueDate = entity.Estimated_Ship_Date__Due_Date_.Value,
								HasLinkedDocument = entity.DocumentId.HasValue,
								IsForecastProposal = entity.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
								HasWriteAccessToLinkedDocument = proposalIdsWithWriteAccess.Contains(entity.ProposalID),
								IsCommercialCustomer = entity.CustomerTypeId == (int)CustomerType.Commercial ||
													   entity.CustomerTypeId == (int)CustomerType.InternationalCommercial,
								HasOrIsRevision = entity.HasOrIsRevision == 1
							}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// A private helper to retrieve all Proposal Ids, to whose linked documents (RDSBs) the user has write access to
		/// </summary>
		/// <param name="ntID">NTID of the user</param>
		/// <param name="dbModel">DB Model</param>
		/// <returns>A collection of proposal ids</returns>
		private static ICollection<int> GetProposalIdsWithLinkedDocumentWriteAccess(string ntID, genTRACEntities dbModel)
		{
			int[] writeAccessRoles = Constants.EDIT_ROLES.Select(x => (int)x).ToArray();

			// a distinct list of proposal ids, to which the user has write access (based on the roles above)
			ICollection<int> proposalIdsWithWriteAccess = dbModel.ProposalUserRoles
				.Where(x => x.genTRACUser.NTID == ntID && writeAccessRoles.Contains(x.RoleID))
				.Select(x => x.ProposalID).Distinct().ToList();

			return proposalIdsWithWriteAccess;
		}

		/// <summary>
		/// Gets the proposals by user.
		/// </summary>
		/// <param name="ntID">The nt identifier.</param>
		/// <param name="includeWorkpaceCreator">Whether the Workspace Creator Role should be included when getting proposals</param>
		/// <returns>A collection of proposals.</returns>
		public ICollection<ProposalDto> GetProposalsByUser(string ntID, bool includeWorkpaceCreator = false)
		{
			ICollection<ProposalDto> toReturn = null;

			List<int> roles = new List<int>() { (int)PtmRole.Pricer, (int)PtmRole.BackupPricer, (int)PtmRole.CostVolumeLead, (int)PtmRole.CoverSheetApprover, (int)PtmRole.PricingVerification, (int)PtmRole.PeerReviewer, (int)PtmRole.LOBEstLead };
			if (includeWorkpaceCreator)
			{
				roles.Add((int)PtmRole.GenBoeWorkspaceCreator);
			}

			using (new StopwatchTimer("ProposalLoader.GetProposalsByUser", this.Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					ICollection<int> proposalIdsWithWriteAccess = GetProposalIdsWithLinkedDocumentWriteAccess(ntID, dbModel);

					toReturn = (from p in dbModel.Proposals
								join Pur in dbModel.ProposalUserRoles.Where(r => r.genTRACUser.NTID == ntID && roles.Contains(r.RoleID)) on p.ProposalID equals Pur.ProposalID
								select new ProposalDto
								{
									Id = p.ProposalID,
									ProposalStatus = (ProposalStatus)p.ProposalStatusID,
									TrackingNumber = p.ProposalTrackingID,
									ForecastedTrackingNumber = p.ForecastedTrackingID,
									ProposalTitle = p.ProposalTitle,
									CustomerType = (CustomerType)p.CustomerTypeID,
									DocumentId = p.DocumentId,
									IsForecastProposal = p.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
									HasWriteAccessToLinkedDocument = proposalIdsWithWriteAccess.Contains(p.ProposalID)
								}).Distinct().ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Upserts a Proposal
		/// </summary>
		/// <param name="dtoToUpsert">Dto to upsert</param>
		/// <returns>Id of the saved Proposal</returns>
		protected override int? Upsert(ProposalDto dtoToUpsert)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.Upsert", Log))
			{
				if (dtoToUpsert != null)
				{
					// save
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						toReturn = dbModel.upsertProposal(
							dtoToUpsert.Id,
							dtoToUpsert.UpdateDate,
							dtoToUpsert.ProposalTitle,
							dtoToUpsert.OTISOpportunityID,
							Convert.ToInt32(dtoToUpsert.ProposalStatus),
							dtoToUpsert.ProposalType,
							false,
							dtoToUpsert.ProgramName,
							dtoToUpsert.Customer,
							(int)dtoToUpsert.CustomerType,
							(int)dtoToUpsert.ISGSRole,
							dtoToUpsert.Request,
							dtoToUpsert.RFPNumber,
							dtoToUpsert.LineOfBusinessID,
							dtoToUpsert.ProgramAreaId,
							(int)dtoToUpsert.PricingTool,
							(int)dtoToUpsert.BoeTool,
							dtoToUpsert.DeliveryDate,
							string.Join(",", dtoToUpsert.CostElementTypeIds),
							dtoToUpsert.EstimatedProposalValue,
							string.Join(",", dtoToUpsert.ContractTypeIds),
							dtoToUpsert.UpdateDateAssigned,
							dtoToUpsert.CreatedByUserId,
							dtoToUpsert.RFPIssuedDate,
							dtoToUpsert.RFPReceivedDate,
							dtoToUpsert.Comments,
							dtoToUpsert.ContractTypeGroup,
							dtoToUpsert.IsScheduleProposal,
							(int)dtoToUpsert.ProposalLocation,
							dtoToUpsert.ProposalLocationName = (dtoToUpsert.ProposalLocation != ProposalLocation.Other) ? string.Empty : dtoToUpsert.ProposalLocationName,
							dtoToUpsert.BoeToolName = (dtoToUpsert.BoeTool != BOETool.Other) ? string.Empty : dtoToUpsert.BoeToolName,
							dtoToUpsert.PricingToolName = (dtoToUpsert.PricingTool != PricingTool.Other) ? string.Empty : dtoToUpsert.PricingToolName,
							(int)((ProposalChecklistType.InternationalCommercial.IsActive() && (dtoToUpsert.CustomerType == CustomerType.InternationalCommercial || dtoToUpsert.CustomerType == CustomerType.StateAndLocal || dtoToUpsert.CustomerType == CustomerType.Commercial)) ? ProposalChecklistType.InternationalCommercial : ProposalChecklistType.Default),
							dtoToUpsert.ChangeChecklist,
							(int)dtoToUpsert.ProgramProposalStatus,
							dtoToUpsert.ProposalClass,
							(int)dtoToUpsert.WorkflowStatus,
							dtoToUpsert.WorkflowStatusLastUpdated,
							dtoToUpsert.LeadEstimatorSignedDate,
							dtoToUpsert.LeadEstimatorSignatureComment,
							dtoToUpsert.CoverSheetApproverSignedDate,
							dtoToUpsert.CoverSheetApproverSignatureComment,
							dtoToUpsert.PricingVerifierSignedDate,
							dtoToUpsert.PricingVerifierSignatureComment,
							dtoToUpsert.IndependentReviewerSignedDate,
							dtoToUpsert.IndependentReviewerSignatureComment,
							dtoToUpsert.LOBEstimatingLeadSignedDate,
							dtoToUpsert.LOBEstimatingLeadSignatureComment,
							dtoToUpsert.ApprovalEmailText,
							dtoToUpsert.RevisedSubmittalDate,
							dtoToUpsert.IsCCPDRequired,
							dtoToUpsert.IsCostVolumeClassified,
							dtoToUpsert.DocumentId,
							dtoToUpsert.ForecastedTrackingNumber,
							dtoToUpsert.TrackingNumber,
							dtoToUpsert.IsForecastProposal,
							dtoToUpsert.AgreementDate,
							dtoToUpsert.CertificationDate,
							(int?)dtoToUpsert.CutOffDateUtilization,
							dtoToUpsert.CertificationTimelineCompleted,
							dtoToUpsert.CertificationLastEmailed,
							dtoToUpsert.NoBidDate,
							dtoToUpsert.IsRevision,
							dtoToUpsert.RevisionOfId,
							(int?)dtoToUpsert.ReasonCertificationNotRequired,
							dtoToUpsert.OtherReasonComment,
							dtoToUpsert.ProposalSetupComments,
							dtoToUpsert.ModExecutedLastEmailed,
							dtoToUpsert.ProposalCompletedDate,
							(int?)dtoToUpsert.ContractActionType,
							dtoToUpsert.ContractActionTypeOtherText,
							(int)dtoToUpsert.CostVolumeTool,
							dtoToUpsert.CostVolumeToolName = dtoToUpsert.CostVolumeTool != CostVolumeTool.Other ? string.Empty : dtoToUpsert.CostVolumeToolName,
							dtoToUpsert.AdditionalClassification
							).FirstOrDefault();
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Deletes a Proposal
		/// </summary>
		/// <param name="dtoToDelete">Proposal to delete.</param>
		/// <returns>Id of the deleted item.</returns>
		protected override int? Delete(ProposalDto dtoToDelete)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.Delete", Log))
			{
				if (dtoToDelete != null)
				{
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						dbModel.deleteProposal(
							dtoToDelete.Id,
							dtoToDelete.UpdateDate);
					}

					toReturn = dtoToDelete.Id;
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Returns a Collection of slim ProposalDtos from the DB, each with only the bare minimum fields required
		/// to perform unit testing and RDSB.  This helps performance for any tests that required fetching all proposals.
		/// </summary>
		/// <returns>Collection of slim Proposal DTOs</returns>
		[DbQuery]
		public ICollection<ProposalDto> GetAllSlim()
		{
			ICollection<ProposalDto> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetAllSlim", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.Proposals
						.Select(x => new ProposalDto
						{
							Id = x.ProposalID,
							DateCreated = x.DateCreated,
							LineOfBusinessID = x.LineOfBusinessID,
							ProgramAreaId = x.ProgramAreaID,
							ProposalStatus = (ProposalStatus)x.ProposalStatusID,
							WorkflowStatus = (WorkflowStatus)x.WorkflowStatus,
							TrackingNumber = x.ProposalTrackingID,
							ForecastedTrackingNumber = x.ForecastedTrackingID,
							ProposalTitle = x.ProposalTitle,
							DocumentId = x.DocumentId,
							CustomerType = (CustomerType)x.CustomerTypeID,
							IsForecastProposal = x.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
							RevisionOfId = x.RevisionOfId
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get the proposal completed date.
		/// </summary>
		/// <param name="inProposalId">proposal id</param>
		/// <returns>date of completed</returns>
		[DbQuery(2)]
		public DateTime? GetProposalCompletedDate(int inProposalId)
		{
			DateTime? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalCompletedDate", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					int completed = (from p in dbModel.Proposals
									 where p.ProposalID == inProposalId
									 select p.ProposalStatusID).First();

					// as long as the proposal is not in progress, grab the approval completed date
					if (completed == (int)ProposalStatus.Completed || completed == (int)ProposalStatus.PendingCertification || completed == (int)ProposalStatus.Revised
						|| completed == (int)ProposalStatus.PendingAward || completed == (int)ProposalStatus.Lost)
					{
						toReturn = (from c in dbModel.ProposalChecklistCompletes
									where c.ProposalID == inProposalId
									select c.SubmitDate).Max();
					}
					else
					{
						toReturn = null;
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get all completed proposals after an optional submit date
		/// </summary>
		/// <param name="cutoffDate">Earliest submit date to get proposals for</param>
		/// <returns>all completed proposals after an optional date</returns>
		[DbQuery]
		public ICollection<ProposalDto> GetAllCompletedProposalsAfterSubmitDate(DateTime? cutoffDate)
		{
			if (cutoffDate == null)
			{
				// if no date given, set to min value to get all
				cutoffDate = DateTime.MinValue;
			}

			ICollection<ProposalDto> toReturn = new Collection<ProposalDto>();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalCompletedDate", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = (from p in dbModel.Proposals
								join c in dbModel.ProposalContractsDatas
								on p.ProposalID equals c.ProposalID
								where (p.ProposalStatusID == (int)ProposalStatus.Completed || p.ProposalStatusID == (int)ProposalStatus.PendingCertification || p.ProposalStatusID == (int)ProposalStatus.PendingAward)
								&& c.CustomerSubmittalDate >= cutoffDate
								select new ProposalDto
								{
									Id = p.ProposalID,
									DateCreated = p.DateCreated,
									LineOfBusinessID = p.LineOfBusinessID,
									ProgramAreaId = p.ProgramAreaID,
									ProposalStatus = (ProposalStatus)p.ProposalStatusID,
									WorkflowStatus = (WorkflowStatus)p.WorkflowStatus,
									TrackingNumber = p.ProposalTrackingID,
									ForecastedTrackingNumber = p.ForecastedTrackingID,
									ProposalTitle = p.ProposalTitle,
									DocumentId = p.DocumentId,
									CustomerType = (CustomerType)p.CustomerTypeID,
									IsForecastProposal = p.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
									ContractActionType = (ContractActionType?)p.ContractActionType,
									ContractActionTypeOtherText = p.ContractActionTypeOtherText
								}).ToCollection();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Updates a proposal status
		/// </summary>
		/// <param name="proposalId">proposal id</param>
		/// <param name="updateDate">proposal update date</param>
		/// <param name="proposalStatus">proposal status</param>
		/// <returns>Id of the saved Proposal</returns>
		public int? UpdateProposalStatus(int proposalId, DateTime updateDate, ProposalStatus proposalStatus)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.UpdateProposalStatus", Log))
			{
				// save
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.updateProposalStatus(
						proposalId,
						updateDate,
						Convert.ToInt32(proposalStatus)).FirstOrDefault();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Updates the proposal forecast email sent on a proposal.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="updateDate">The update date.</param>
		public void UpdateProposalForecastEmailSent(int proposalId, DateTime updateDate)
		{
			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.UpdateProposalForecastEmailSent", Log))
			{
				// save
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					dbModel.updateProposalForecastEmailSent(
						proposalId,
						updateDate);
				}
			}
		}

		/// <summary>
		/// Checks if the given proposal title is unique compared to all proposals.
		/// If the proposal title already exists, check if the proposal ids match.
		/// </summary>
		/// <param name="proposalId">proposal id</param>
		/// <param name="proposalTitle">name of proposal</param>
		/// <returns>true or false</returns>
		[DbQuery]
		public bool IsProposalTitleUnique(int proposalId, string proposalTitle)
		{
			if (proposalTitle == null)
			{
				throw new ArgumentException("proposalTitle is null");
			}

			using (genTRACEntities dbModel = new genTRACEntities())
			{
				System.Data.Entity.DbSet<Proposal> proposals = dbModel.Proposals;

				// If another proposal has this title and the ids don't match, then it's a different proposal
				bool otherProposalHaveThisTitle = proposals.Where(x => (x.ProposalTitle.ToLower() == proposalTitle.ToLower())
					&& (x.ProposalID != proposalId)).Any();

				// Negate since we're checking for uniqueness
				return !otherProposalHaveThisTitle;
			}
		}

		/// <summary>
		/// Get the proposal status
		/// </summary>
		/// <param name="inProposalId">proposal id</param>
		/// <returns>proposal status</returns>
		[DbQuery]
		public ProposalStatus? GetProposalStatus(int inProposalId)
		{
			ProposalStatus? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalStatus", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = (ProposalStatus?)(from p in dbModel.Proposals
												 where p.ProposalID == inProposalId
												 select p.ProposalStatusID).FirstOrDefault();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method.
		/// </summary>
		/// <param name="workflowStatus">The workflow status.</param>
		/// <returns>A list of proposals.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ICollection<ProposalDto> GetProposalsByWorkflowStatus(WorkflowStatus workflowStatus)
		{
			ICollection<ProposalDto> toReturn = new List<ProposalDto>();

			int workflowStatusAsInteger = (int)workflowStatus;
			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalsByWorkflowStatus", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = toReturn = dbModel.Proposals.Where(p => p.WorkflowStatus == workflowStatusAsInteger).Select(entity => new
					{
						Id = entity.ProposalID,
						TrackingNumber = entity.ProposalTrackingID,
						ProposalTitle = entity.ProposalTitle,
						OTISOpportunityID = entity.OTISOpportunityID,
						ProposalStatus = (ProposalStatus)entity.ProposalStatusID,
						UpdateDate = entity.UpdateDate,
						ProposalType = entity.ProposalTypeID,
						ProgramName = entity.ProgramName,
						Customer = entity.Customer,
						ISGSRole = (ISGSRole)entity.ISGSRoleID,
						Request = entity.RequestTypeID,
						ProposalClass = entity.ProposalClassID,
						RFPNumber = entity.RFPNumber,
						LineOfBusinessID = entity.LineOfBusinessID,
						ProgramAreaId = entity.ProgramAreaID,
						ProposalLocation = (ProposalLocation)entity.ProposalLocationID,
						ProposalLocationName = entity.ProposalLocationName,
						PricingTool = (PricingTool)entity.PricingToolID,
						PricingToolName = entity.PricingToolName,
						BoeTool = (BOETool)entity.BOEToolID,
						BoeToolName = entity.BOEToolName,
						DeliveryDate = entity.AnticipatedDeliveryDate,
						RevisedSubmittalDate = entity.RevisedSubmittalDate,
						EstimatedProposalValue = entity.EstimatedProposalValue,
						CustomerType = (CustomerType)entity.CustomerTypeID,
						DateAssigned = entity.DateAssigned,
						DateCreated = entity.DateCreated,
						RFPIssuedDate = entity.RFPIssuedDate,
						RFPReceivedDate = entity.RFPReceivedDate,
						Comments = entity.Comments,
						ContractTypeGroup = entity.ContractTypeGroupID.HasValue ? entity.ContractTypeGroupID.Value : 0, // 0 is Not Set
						CreatedByUserId = entity.CreatedByUserID,
						IsScheduleProposal = entity.IsScheduleProposal,
						ProgramProposalStatus = entity.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
						WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
						WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
						LeadEstimatorSignedDate = entity.LeadEstimatorSignedDT,
						LeadEstimatorSignatureComment = entity.LeadEstimatorSignComment,
						CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDT,
						CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignComment,
						PricingVerifierSignedDate = entity.PricingVerifierSignedDT,
						PricingVerifierSignatureComment = entity.PricingVerifierSignComment,
						IndependentReviewerSignedDate = entity.IndependentReviewerSignedDT,
						IndependentReviewerSignatureComment = entity.IndependentReviewerSignComment,
						LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDT,
						LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignComment,
						ApprovalEmailText = entity.ApprovalEmailText,
						IsCCPDRequired = entity.CCPDRequired,
						IsCostVolumeClassified = entity.CostVolumeClassified,
						DocumentId = entity.DocumentId,
						ForecastedTrackingNumber = entity.ForecastedTrackingID,
						IsForecastProposal = entity.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
						ForecastEmailSent = entity.ForecastEmailSent,
						ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
						CostElementTypeIds = entity.CostElementLUs.Select(x => x.CostElementID),
						AgreementDate = entity.AgreementDate,
						CertificationDate = entity.CertificationDate,
						CutOffDateUtilization = entity.CutOffDateUtilization,
						CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
						CertificationLastEmailed = entity.CertificationLastEmailed,
						NoBidDate = entity.NoBidDate,
						RevisionOfId = entity.RevisionOfId,
						entity.ReasonCertificationNotRequired,
						entity.OtherReasonComment,
						ProposalSetupComments = entity.SetupComments,
						InformationComments = entity.InformationComments,
						ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
						ContractActionType = entity.ContractActionType,
						ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
						CostVolumeTool = (CostVolumeTool)entity.CostVolumeToolID,
						CostVolumeToolName = entity.CostVolumeToolName,
						AdditionalClassification = entity.AdditionalClassification
					}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}


		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method.
		/// </summary>
		/// <param name="workflowStatus">The proposal status.</param>
		/// <returns>A list of proposals.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ICollection<ProposalDto> GetProposalsByProposalStatus(ProposalStatus proposalStatus)
		{
			ICollection<ProposalDto> toReturn = new List<ProposalDto>();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalsByWorkflowStatus", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = toReturn = dbModel.Proposals.Where(p => p.ProposalStatusID == (int)proposalStatus).Select(entity => new
					{
						Id = entity.ProposalID,
						TrackingNumber = entity.ProposalTrackingID,
						ProposalTitle = entity.ProposalTitle,
						OTISOpportunityID = entity.OTISOpportunityID,
						ProposalStatus = (ProposalStatus)entity.ProposalStatusID,
						UpdateDate = entity.UpdateDate,
						ProposalType = entity.ProposalTypeID,
						ProgramName = entity.ProgramName,
						Customer = entity.Customer,
						ISGSRole = (ISGSRole)entity.ISGSRoleID,
						Request = entity.RequestTypeID,
						ProposalClass = entity.ProposalClassID,
						RFPNumber = entity.RFPNumber,
						LineOfBusinessID = entity.LineOfBusinessID,
						ProgramAreaId = entity.ProgramAreaID,
						ProposalLocation = (ProposalLocation)entity.ProposalLocationID,
						ProposalLocationName = entity.ProposalLocationName,
						PricingTool = (PricingTool)entity.PricingToolID,
						PricingToolName = entity.PricingToolName,
						BoeTool = (BOETool)entity.BOEToolID,
						BoeToolName = entity.BOEToolName,
						DeliveryDate = entity.AnticipatedDeliveryDate,
						RevisedSubmittalDate = entity.RevisedSubmittalDate,
						EstimatedProposalValue = entity.EstimatedProposalValue,
						CustomerType = (CustomerType)entity.CustomerTypeID,
						DateAssigned = entity.DateAssigned,
						DateCreated = entity.DateCreated,
						RFPIssuedDate = entity.RFPIssuedDate,
						RFPReceivedDate = entity.RFPReceivedDate,
						Comments = entity.Comments,
						ContractTypeGroup = entity.ContractTypeGroupID.HasValue ? entity.ContractTypeGroupID.Value : 0, // 0 is Not Set
						CreatedByUserId = entity.CreatedByUserID,
						IsScheduleProposal = entity.IsScheduleProposal,
						ProgramProposalStatus = entity.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
						WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
						WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
						LeadEstimatorSignedDate = entity.LeadEstimatorSignedDT,
						LeadEstimatorSignatureComment = entity.LeadEstimatorSignComment,
						CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDT,
						CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignComment,
						PricingVerifierSignedDate = entity.PricingVerifierSignedDT,
						PricingVerifierSignatureComment = entity.PricingVerifierSignComment,
						IndependentReviewerSignedDate = entity.IndependentReviewerSignedDT,
						IndependentReviewerSignatureComment = entity.IndependentReviewerSignComment,
						LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDT,
						LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignComment,
						ApprovalEmailText = entity.ApprovalEmailText,
						IsCCPDRequired = entity.CCPDRequired,
						IsCostVolumeClassified = entity.CostVolumeClassified,
						DocumentId = entity.DocumentId,
						ForecastedTrackingNumber = entity.ForecastedTrackingID,
						IsForecastProposal = entity.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
						ForecastEmailSent = entity.ForecastEmailSent,
						ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
						CostElementTypeIds = entity.CostElementLUs.Select(x => x.CostElementID),
						AgreementDate = entity.AgreementDate,
						CertificationDate = entity.CertificationDate,
						CutOffDateUtilization = entity.CutOffDateUtilization,
						CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
						CertificationLastEmailed = entity.CertificationLastEmailed,
						NoBidDate = entity.NoBidDate,
						RevisionOfId = entity.RevisionOfId,
						entity.ReasonCertificationNotRequired,
						entity.OtherReasonComment,
						ProposalSetupComments = entity.SetupComments,
						InformationComments = entity.InformationComments,
						ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
						ContractActionType = entity.ContractActionType,
						ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
						CostVolumeTool = (CostVolumeTool)entity.CostVolumeToolID,
						CostVolumeToolName = entity.CostVolumeToolName,
						AdditionalClassification = entity.AdditionalClassification
					}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method and older than the cutoff date.
		/// </summary>
		/// <param name="workflowStatus">The workflow status.</param>
		/// <param name="cutoffDate">The cutoff date for retrieving proposals.</param>
		/// <returns>A list of proposals.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ICollection<ProposalDto> GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus workflowStatus, DateTime cutoffDate)
		{
			ICollection<ProposalDto> toReturn = new List<ProposalDto>();

			int workflowStatusAsInteger = (int)workflowStatus;
			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = toReturn = dbModel.Proposals.Where(p => p.WorkflowStatus == workflowStatusAsInteger && p.WorkflowStatusLastUpdated < cutoffDate)
						.Select(entity => new
						{
							Id = entity.ProposalID,
							TrackingNumber = entity.ProposalTrackingID,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = (ProposalStatus)entity.ProposalStatusID,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalTypeID,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = (ISGSRole)entity.ISGSRoleID,
							Request = entity.RequestTypeID,
							ProposalClass = entity.ProposalClassID,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaID,
							ProposalLocation = (ProposalLocation)entity.ProposalLocationID,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = (PricingTool)entity.PricingToolID,
							PricingToolName = entity.PricingToolName,
							BoeTool = (BOETool)entity.BOEToolID,
							BoeToolName = entity.BOEToolName,
							DeliveryDate = entity.AnticipatedDeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = (CustomerType)entity.CustomerTypeID,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroupID.HasValue ? entity.ContractTypeGroupID.Value : 0, // 0 is Not Set
							CreatedByUserId = entity.CreatedByUserID,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
							WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDT,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDT,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDT,
							PricingVerifierSignatureComment = entity.PricingVerifierSignComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDT,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDT,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.CCPDRequired,
							IsCostVolumeClassified = entity.CostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingID,
							IsForecastProposal = entity.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
							CostElementTypeIds = entity.CostElementLUs.Select(x => x.CostElementID),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							entity.ReasonCertificationNotRequired,
							entity.OtherReasonComment,
							ProposalSetupComments = entity.SetupComments,
							InformationComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = (CostVolumeTool)entity.CostVolumeToolID,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets the next forecasted tracking number.
		/// </summary>
		/// <returns>The next forecasted tracking number.</returns>
		[DbQuery]
		public string GetNextForecastedTrackingNumber()
		{
			// format: F-YY-xxx 
			// F is a constant(as in Forecasted)
			// YY being last 2 digits of a year
			// xxx being the number for the year

			string currentYear = string.Format(FORECAST_NUMBER_FORMAT, DateTime.Now.ToString("yy"));

			return currentYear + NextGenerationNumber.ToString();
		}

		/// <summary>
		/// Gets the forecast proposals past allowed date.
		/// </summary>
		/// <param name="cutoffDate">The cutoff date.</param>
		/// <returns>A list of proposals.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery]
		public ICollection<ProposalDto> GetForecastProposalsPastAllowedDate(DateTime cutoffDate)
		{
			ICollection<ProposalDto> toReturn = new List<ProposalDto>();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetForecastProposalsPastAllowedDate", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.Proposals.Where(p => !p.ForecastEmailSent && p.AnticipatedDeliveryDate < cutoffDate && p.ForecastedTrackingID != null
													&& p.ForecastedTrackingID != string.Empty && (p.ProposalTrackingID == null || p.ProposalTrackingID == string.Empty))
						.Select(entity => new
						{
							Id = entity.ProposalID,
							TrackingNumber = entity.ProposalTrackingID,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = (ProposalStatus)entity.ProposalStatusID,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalTypeID,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = (ISGSRole)entity.ISGSRoleID,
							Request = entity.RequestTypeID,
							ProposalClass = entity.ProposalClassID,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaID,
							ProposalLocation = (ProposalLocation)entity.ProposalLocationID,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = (PricingTool)entity.PricingToolID,
							PricingToolName = entity.PricingToolName,
							BoeTool = (BOETool)entity.BOEToolID,
							BoeToolName = entity.BOEToolName,
							DeliveryDate = entity.AnticipatedDeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = (CustomerType)entity.CustomerTypeID,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroupID.HasValue ? entity.ContractTypeGroupID.Value : 0, // 0 is Not Set
							CreatedByUserId = entity.CreatedByUserID,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
							WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDT,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDT,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDT,
							PricingVerifierSignatureComment = entity.PricingVerifierSignComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDT,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDT,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.CCPDRequired,
							IsCostVolumeClassified = entity.CostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingID,
							IsForecastProposal = entity.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
							CostElementTypeIds = entity.CostElementLUs.Select(x => x.CostElementID),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							entity.ReasonCertificationNotRequired,
							entity.OtherReasonComment,
							ProposalSetupComments = entity.SetupComments,
							InformationComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = (CostVolumeTool)entity.CostVolumeToolID,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets the list of proposals that require the Mod Execution Missing email
		/// </summary>
		/// <returns>List of proposal DTOs</returns>
		[DbQuery]
		public ICollection<ProposalDto> GetModExecutedDateMissingNotifications()
		{
			List<ProposalDto> toReturn = new List<ProposalDto>();
			DateTime lastSentThreshold = DateTime.Now.AddDays(-7);
			DateTime initialSendThreshold = DateTime.Now.AddDays(-15);
			DateTime featureStartDate = DateTime.Parse(ConfigurationUtilities.GetAppSetting("PtmContractsStartDate"));

			using (genTRACEntities context = new genTRACEntities())
			{
				var data = context.Proposals.GroupJoin(context.ProposalContractsDatas, p => p.ProposalID, c => c.ProposalID, (p, c) => new { p, c })
						.SelectMany(prop => prop.c.DefaultIfEmpty(), (prop, cont) => new { Proposals = prop.p, Contracts = cont })
						.Where(x =>
							(x.Contracts.ModCompletedDate == null
								&& x.Contracts.LmWon != false
								&& x.Proposals.CertificationDate > featureStartDate
							) // Overall filtering - Mod date missing, proposal not marked as lost, proposal was completed after the feature was deployed
							&&
								((x.Proposals.CertificationDate != null
									&& x.Proposals.ModExecutedLastEmailed == null
									&& x.Proposals.CertificationDate <= initialSendThreshold
								 ) // initial send logic
							 || (x.Proposals.ModExecutedLastEmailed != null
									&& lastSentThreshold >= x.Proposals.ModExecutedLastEmailed)
								)) // re-send logic
						.ToList();

				toReturn = data.Select(x => new ProposalDto
				{
					Id = x.Proposals.ProposalID,
					TrackingNumber = x.Proposals.ProposalTrackingID,
					ProposalTitle = x.Proposals.ProposalTitle,
					OTISOpportunityID = x.Proposals.OTISOpportunityID,
					ProposalStatus = (ProposalStatus)x.Proposals.ProposalStatusID,
					UpdateDate = x.Proposals.UpdateDate,
					ProposalType = x.Proposals.ProposalTypeID,
					ProgramName = x.Proposals.ProgramName,
					Customer = x.Proposals.Customer,
					ISGSRole = (ISGSRole)x.Proposals.ISGSRoleID,
					Request = x.Proposals.RequestTypeID,
					ProposalClass = x.Proposals.ProposalClassID,
					RFPNumber = x.Proposals.RFPNumber,
					LineOfBusinessID = x.Proposals.LineOfBusinessID,
					ProgramAreaId = x.Proposals.ProgramAreaID,
					ProposalLocation = (ProposalLocation)x.Proposals.ProposalLocationID,
					ProposalLocationName = x.Proposals.ProposalLocationName,
					PricingTool = (PricingTool)x.Proposals.PricingToolID,
					PricingToolName = x.Proposals.PricingToolName,
					BoeTool = (BOETool)x.Proposals.BOEToolID,
					BoeToolName = x.Proposals.BOEToolName,
					DeliveryDate = x.Proposals.AnticipatedDeliveryDate,
					RevisedSubmittalDate = x.Proposals.RevisedSubmittalDate,
					EstimatedProposalValue = x.Proposals.EstimatedProposalValue,
					CustomerType = (CustomerType)x.Proposals.CustomerTypeID,
					DateAssigned = x.Proposals.DateAssigned,
					DateCreated = x.Proposals.DateCreated,
					RFPIssuedDate = x.Proposals.RFPIssuedDate,
					RFPReceivedDate = x.Proposals.RFPReceivedDate,
					Comments = x.Proposals.Comments,
					ContractTypeGroup = x.Proposals.ContractTypeGroupID.HasValue ? x.Proposals.ContractTypeGroupID.Value : 0, // 0 is Not Set
					CreatedByUserId = x.Proposals.CreatedByUserID,
					IsScheduleProposal = x.Proposals.IsScheduleProposal,
					ProgramProposalStatus = x.Proposals.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)x.Proposals.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
					WorkflowStatus = (WorkflowStatus)x.Proposals.WorkflowStatus,
					WorkflowStatusLastUpdated = x.Proposals.WorkflowStatusLastUpdated,
					LeadEstimatorSignedDate = x.Proposals.LeadEstimatorSignedDT,
					LeadEstimatorSignatureComment = x.Proposals.LeadEstimatorSignComment,
					CoverSheetApproverSignedDate = x.Proposals.CoverSheetApproverSignedDT,
					CoverSheetApproverSignatureComment = x.Proposals.CoverSheetApproverSignComment,
					PricingVerifierSignedDate = x.Proposals.PricingVerifierSignedDT,
					PricingVerifierSignatureComment = x.Proposals.PricingVerifierSignComment,
					IndependentReviewerSignedDate = x.Proposals.IndependentReviewerSignedDT,
					IndependentReviewerSignatureComment = x.Proposals.IndependentReviewerSignComment,
					LOBEstimatingLeadSignedDate = x.Proposals.LOBEstimatingLeadSignedDT,
					LOBEstimatingLeadSignatureComment = x.Proposals.LOBEstimatingLeadSignComment,
					ApprovalEmailText = x.Proposals.ApprovalEmailText,
					IsCCPDRequired = x.Proposals.CCPDRequired,
					IsCostVolumeClassified = x.Proposals.CostVolumeClassified,
					DocumentId = x.Proposals.DocumentId,
					ForecastedTrackingNumber = x.Proposals.ForecastedTrackingID,
					IsForecastProposal = x.Proposals.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
					ForecastEmailSent = x.Proposals.ForecastEmailSent,
					ContractTypeIds = x.Proposals.ContractTypeLUs.Select(y => y.ContractTypeID).ToList(),
					CostElementTypeIds = x.Proposals.CostElementLUs.Select(y => y.CostElementID).ToList(),
					AgreementDate = x.Proposals.AgreementDate,
					CertificationDate = x.Proposals.CertificationDate,
					CutOffDateUtilization = x.Proposals.CutOffDateUtilization == null ? (CutOffDateUtilization?)null : (CutOffDateUtilization)x.Proposals.CutOffDateUtilization,
					CertificationTimelineCompleted = x.Proposals.CertificationTimelineCompleted,
					CertificationLastEmailed = x.Proposals.CertificationLastEmailed,
					NoBidDate = x.Proposals.NoBidDate,
					RevisionOfId = x.Proposals.RevisionOfId,
					ReasonCertificationNotRequired = x.Proposals.ReasonCertificationNotRequired == null ? (ReasonCertificationNotRequired?)null : (ReasonCertificationNotRequired)x.Proposals.ReasonCertificationNotRequired,
					OtherReasonComment = x.Proposals.OtherReasonComment,
					ProposalSetupComments = x.Proposals.SetupComments,
					ManageProposalInfoComments = x.Proposals.InformationComments,
					ModExecutedLastEmailed = x.Proposals.ModExecutedLastEmailed,
					ContractActionType = (ContractActionType?)x.Proposals.ContractActionType,
					ContractActionTypeOtherText = x.Proposals.ContractActionTypeOtherText,
					CostVolumeTool = (CostVolumeTool)x.Proposals.CostVolumeToolID,
					CostVolumeToolName = x.Proposals.CostVolumeToolName,
					AdditionalClassification = x.Proposals.AdditionalClassification
				}).ToList();
			}

			return toReturn;
		}

		/// <summary>
		/// Gets the proposals certification timeline past due.
		/// </summary>
		/// <returns>A list of proposals.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery]
		public ICollection<ProposalDto> GetProposalsCertificationTimelinePastDue()
		{
			ICollection<ProposalDto> toReturn = new List<ProposalDto>();
			DateTime now = DateTime.Now.Date;
			DateTime thirtyDaysAgo = now.AddDays(-30);
			DateTime sixtyDaysAgo = now.AddDays(-60);
			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalsCertificationTimelinePastDue", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					// Retrieves list of proposals and associated contracts
					var proposalsList = dbModel.Proposals.GroupJoin(dbModel.ProposalContractsDatas, p => p.ProposalID, c => c.ProposalID, (p, c) => new { p, c })
					   .SelectMany(prop => prop.c.DefaultIfEmpty(), (prop, cont) => new { Proposals = prop.p, Contracts = cont })
					   .Where(x => (x.Proposals.ProposalStatusID == (int)ProposalStatus.PendingCertification
									&& (x.Proposals.CertificationLastEmailed == null || x.Proposals.CertificationLastEmailed < thirtyDaysAgo)
									&& (!x.Proposals.ProposalChecklists.Any() || x.Contracts.CustomerSubmittalDate < sixtyDaysAgo)));

					toReturn = proposalsList.Select(entity => new
					{
						Id = entity.Proposals.ProposalID,
						TrackingNumber = entity.Proposals.ProposalTrackingID,
						ProposalTitle = entity.Proposals.ProposalTitle,
						OTISOpportunityID = entity.Proposals.OTISOpportunityID,
						ProposalStatus = (ProposalStatus)entity.Proposals.ProposalStatusID,
						UpdateDate = entity.Proposals.UpdateDate,
						ProposalType = entity.Proposals.ProposalTypeID,
						ProgramName = entity.Proposals.ProgramName,
						Customer = entity.Proposals.Customer,
						ISGSRole = (ISGSRole)entity.Proposals.ISGSRoleID,
						Request = entity.Proposals.RequestTypeID,
						ProposalClass = entity.Proposals.ProposalClassID,
						RFPNumber = entity.Proposals.RFPNumber,
						LineOfBusinessID = entity.Proposals.LineOfBusinessID,
						ProgramAreaId = entity.Proposals.ProgramAreaID,
						ProposalLocation = (ProposalLocation)entity.Proposals.ProposalLocationID,
						ProposalLocationName = entity.Proposals.ProposalLocationName,
						PricingTool = (PricingTool)entity.Proposals.PricingToolID,
						PricingToolName = entity.Proposals.PricingToolName,
						BoeTool = (BOETool)entity.Proposals.BOEToolID,
						BoeToolName = entity.Proposals.BOEToolName,
						DeliveryDate = entity.Proposals.AnticipatedDeliveryDate,
						RevisedSubmittalDate = entity.Proposals.RevisedSubmittalDate,
						EstimatedProposalValue = entity.Proposals.EstimatedProposalValue,
						CustomerType = (CustomerType)entity.Proposals.CustomerTypeID,
						DateAssigned = entity.Proposals.DateAssigned,
						DateCreated = entity.Proposals.DateCreated,
						RFPIssuedDate = entity.Proposals.RFPIssuedDate,
						RFPReceivedDate = entity.Proposals.RFPReceivedDate,
						Comments = entity.Proposals.Comments,
						ContractTypeGroup = entity.Proposals.ContractTypeGroupID.HasValue ? entity.Proposals.ContractTypeGroupID.Value : 0, // 0 is Not Set
						CreatedByUserId = entity.Proposals.CreatedByUserID,
						IsScheduleProposal = entity.Proposals.IsScheduleProposal,
						ProgramProposalStatus = entity.Proposals.ProgramProposalStatusID.HasValue ? (ProgramProposalStatus)entity.Proposals.ProgramProposalStatusID.Value : ProgramProposalStatus.NotSet,
						WorkflowStatus = (WorkflowStatus)entity.Proposals.WorkflowStatus,
						WorkflowStatusLastUpdated = entity.Proposals.WorkflowStatusLastUpdated,
						LeadEstimatorSignedDate = entity.Proposals.LeadEstimatorSignedDT,
						LeadEstimatorSignatureComment = entity.Proposals.LeadEstimatorSignComment,
						CoverSheetApproverSignedDate = entity.Proposals.CoverSheetApproverSignedDT,
						CoverSheetApproverSignatureComment = entity.Proposals.CoverSheetApproverSignComment,
						PricingVerifierSignedDate = entity.Proposals.PricingVerifierSignedDT,
						PricingVerifierSignatureComment = entity.Proposals.PricingVerifierSignComment,
						IndependentReviewerSignedDate = entity.Proposals.IndependentReviewerSignedDT,
						IndependentReviewerSignatureComment = entity.Proposals.IndependentReviewerSignComment,
						LOBEstimatingLeadSignedDate = entity.Proposals.LOBEstimatingLeadSignedDT,
						LOBEstimatingLeadSignatureComment = entity.Proposals.LOBEstimatingLeadSignComment,
						ApprovalEmailText = entity.Proposals.ApprovalEmailText,
						IsCCPDRequired = entity.Proposals.CCPDRequired,
						IsCostVolumeClassified = entity.Proposals.CostVolumeClassified,
						DocumentId = entity.Proposals.DocumentId,
						ForecastedTrackingNumber = entity.Proposals.ForecastedTrackingID,
						IsForecastProposal = entity.Proposals.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED,
						ForecastEmailSent = entity.Proposals.ForecastEmailSent,
						ContractTypeIds = entity.Proposals.ContractTypeLUs.Select(x => x.ContractTypeID),
						CostElementTypeIds = entity.Proposals.CostElementLUs.Select(x => x.CostElementID),
						AgreementDate = entity.Proposals.AgreementDate,
						CertificationDate = entity.Proposals.CertificationDate,
						CutOffDateUtilization = entity.Proposals.CutOffDateUtilization,
						CertificationTimelineCompleted = entity.Proposals.CertificationTimelineCompleted,
						CertificationLastEmailed = entity.Proposals.CertificationLastEmailed,
						NoBidDate = entity.Proposals.NoBidDate,
						RevisionOfId = entity.Proposals.RevisionOfId,
						entity.Proposals.ReasonCertificationNotRequired,
						entity.Proposals.OtherReasonComment,
						ProposalSetupComments = entity.Proposals.SetupComments,
						InformationComments = entity.Proposals.InformationComments,
						ModExecutedLastEmailed = entity.Proposals.ModExecutedLastEmailed,
						ContractActionType = entity.Proposals.ContractActionType,
						ContractActionTypeOtherText = entity.Proposals.ContractActionTypeOtherText,
						CostVolumeTool = (CostVolumeTool)entity.Proposals.CostVolumeToolID,
						CostVolumeToolName = entity.Proposals.CostVolumeToolName,
						AdditionalClassification = entity.Proposals.AdditionalClassification
					}).ToList()
						.Select(entity => new ProposalDto() // this is needed to deal w/ the .ToList()
						{
							Id = entity.Id,
							TrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							OTISOpportunityID = entity.OTISOpportunityID,
							ProposalStatus = entity.ProposalStatus,
							UpdateDate = entity.UpdateDate,
							ProposalType = entity.ProposalType,
							ProgramName = entity.ProgramName,
							Customer = entity.Customer,
							ISGSRole = entity.ISGSRole,
							Request = entity.Request,
							ProposalClass = entity.ProposalClass,
							RFPNumber = entity.RFPNumber,
							LineOfBusinessID = entity.LineOfBusinessID,
							ProgramAreaId = entity.ProgramAreaId,
							ProposalLocation = entity.ProposalLocation,
							ProposalLocationName = entity.ProposalLocationName,
							PricingTool = entity.PricingTool,
							PricingToolName = entity.PricingToolName,
							BoeTool = entity.BoeTool,
							BoeToolName = entity.BoeToolName,
							DeliveryDate = entity.DeliveryDate,
							RevisedSubmittalDate = entity.RevisedSubmittalDate,
							EstimatedProposalValue = entity.EstimatedProposalValue,
							CustomerType = entity.CustomerType,
							DateAssigned = entity.DateAssigned,
							DateCreated = entity.DateCreated,
							RFPIssuedDate = entity.RFPIssuedDate,
							RFPReceivedDate = entity.RFPReceivedDate,
							Comments = entity.Comments,
							ContractTypeGroup = entity.ContractTypeGroup,
							CreatedByUserId = entity.CreatedByUserId,
							IsScheduleProposal = entity.IsScheduleProposal,
							ProgramProposalStatus = entity.ProgramProposalStatus,
							WorkflowStatus = entity.WorkflowStatus,
							WorkflowStatusLastUpdated = entity.WorkflowStatusLastUpdated,
							LeadEstimatorSignedDate = entity.LeadEstimatorSignedDate,
							LeadEstimatorSignatureComment = entity.LeadEstimatorSignatureComment,
							CoverSheetApproverSignedDate = entity.CoverSheetApproverSignedDate,
							CoverSheetApproverSignatureComment = entity.CoverSheetApproverSignatureComment,
							PricingVerifierSignedDate = entity.PricingVerifierSignedDate,
							PricingVerifierSignatureComment = entity.PricingVerifierSignatureComment,
							IndependentReviewerSignedDate = entity.IndependentReviewerSignedDate,
							IndependentReviewerSignatureComment = entity.IndependentReviewerSignatureComment,
							LOBEstimatingLeadSignedDate = entity.LOBEstimatingLeadSignedDate,
							LOBEstimatingLeadSignatureComment = entity.LOBEstimatingLeadSignatureComment,
							ApprovalEmailText = entity.ApprovalEmailText,
							IsCCPDRequired = entity.IsCCPDRequired,
							IsCostVolumeClassified = entity.IsCostVolumeClassified,
							DocumentId = entity.DocumentId,
							ForecastedTrackingNumber = entity.ForecastedTrackingNumber,
							IsForecastProposal = entity.IsForecastProposal,
							ForecastEmailSent = entity.ForecastEmailSent,
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							CostElementTypeIds = entity.CostElementTypeIds.ToList(),
							AgreementDate = entity.AgreementDate,
							CertificationDate = entity.CertificationDate,
							CutOffDateUtilization = entity.CutOffDateUtilization.HasValue ? (CutOffDateUtilization?)entity.CutOffDateUtilization : null,
							CertificationTimelineCompleted = entity.CertificationTimelineCompleted,
							CertificationLastEmailed = entity.CertificationLastEmailed,
							NoBidDate = entity.NoBidDate,
							RevisionOfId = entity.RevisionOfId,
							ReasonCertificationNotRequired = (ReasonCertificationNotRequired?)entity.ReasonCertificationNotRequired,
							OtherReasonComment = entity.OtherReasonComment,
							ProposalSetupComments = entity.ProposalSetupComments,
							ManageProposalInfoComments = entity.InformationComments,
							ModExecutedLastEmailed = entity.ModExecutedLastEmailed,
							ContractActionType = (ContractActionType?)entity.ContractActionType,
							ContractActionTypeOtherText = entity.ContractActionTypeOtherText,
							CostVolumeTool = entity.CostVolumeTool,
							CostVolumeToolName = entity.CostVolumeToolName,
							AdditionalClassification = entity.AdditionalClassification
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Retrieves the next generation number for this year.
		/// </summary>
		/// <returns>The next generation number for this year.</returns>
		private static int RetrieveNextGenerationNumber()
		{
			// format: F-YY-xxx 
			// F is a constant(as in Forecasted)
			// YY being last 2 digits of a year
			// xxx being the number for the year

			string currentYear = string.Format(FORECAST_NUMBER_FORMAT, DateTime.Now.ToString("yy"));
			int max = 0;

			ICollection<string> forecastedTrackingNumbers = null;

			using (genTRACEntities dbModel = new genTRACEntities())
			{
				// select the Proposal IDs from the database
				IQueryable<string> forecastedTracking = from x in dbModel.Proposals
														where x.ForecastedTrackingID.StartsWith(currentYear)
														select x.ForecastedTrackingID;

				forecastedTrackingNumbers = forecastedTracking.ToList();
			}

			// cannot use count to find the max in case a proposal was deleted
			foreach (string id in forecastedTrackingNumbers)
			{
				int num = int.Parse(id.Substring(5));
				max = Math.Max(max, num);
			}

			// Add one to grow on
			max++;

			return max;
		}

		/// <summary>
		/// Gets Revision History for the specific proposal
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Revision History</returns>
		[DbQuery]
		public ICollection<RevisionHistoryModelView> GetRevisionHistory(int proposalId)
		{
			ICollection<RevisionHistoryModelView> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetRevisionHistory", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					IEnumerable<int?> proposalIds = dbModel.GetProposalRevisionHistory(proposalId).Select(x => x.ProposalId);

					toReturn = dbModel.Proposals.Where(x => proposalIds.Contains(x.ProposalID) && x.ProposalStatusID != (int)ProposalStatus.Deleted)
						.Select(x => new
						{
							x.ProposalID,
							x.ProposalTrackingID,
							x.ProposalTitle,
							x.AnticipatedDeliveryDate,
							x.RevisedSubmittalDate,
							ProposalStatus = (ProposalStatus)x.ProposalStatusID,
							MaxCompleteDate = x.ProposalChecklistCompletes.Max(z => z.SubmitDate),
							x.CCPDRequired,
							x.CertificationTimelineCompleted,
							IsRevision = x.RevisionOfId != null
						}).ToList()
						// some of the more complicating operations (below) need to be done in C#, not in SQL, hence the approach
						.Select(x => new RevisionHistoryModelView()
						{
							ProposalId = x.ProposalID,
							IsCurrentlySelected = x.ProposalID == proposalId,
							TrackingNumber = x.ProposalTrackingID,
							ProposalTitle = x.ProposalTitle,
							WorkflowCompletedLine = GetWorkflowCompletedLineText(x.ProposalStatus, x.AnticipatedDeliveryDate, x.MaxCompleteDate, x.RevisedSubmittalDate),
							CertificationCompletedLine = GetCertificationCompletedLineText(x.ProposalStatus, x.CCPDRequired, x.CertificationTimelineCompleted),

							DisplayProposalSetupTab = true,
							DisplayChecklistTab = true,
							DisplayPSATab = true,
							DisplayApprovalsTab = true,
							DisplayCertificationTab = this.DisplayCertificationTab(x.ProposalStatus, x.CCPDRequired),
							DisplayRevisionTab = this.DisplayRevisionTab(x.IsRevision, x.ProposalStatus)
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets Workspace Completed Line for the Revision History
		/// </summary>
		/// <param name="proposalStatus">Proposal Status</param>
		/// <param name="anticipatedDeliveryDate">Anticipated Delivery Date</param>
		/// <param name="maxCompleteDate">Max Completed Date</param>
		/// <returns>Text for the WS completed line</returns>
		public static string GetWorkflowCompletedLineText(ProposalStatus proposalStatus, DateTime anticipatedDeliveryDate, DateTime? maxCompleteDate, DateTime? revisedDeliveryDate)
		{
			string result = string.Empty;

			switch (proposalStatus)
			{
				case ProposalStatus.InProgress:
					result = "Due: " + (revisedDeliveryDate.HasValue ? revisedDeliveryDate.Value.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)
						: anticipatedDeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR));
					break;
				case ProposalStatus.Completed:
				case ProposalStatus.PendingCertification:
				case ProposalStatus.PendingAward:
				case ProposalStatus.Lost:
				case ProposalStatus.Revised:
					result = "Approval Workflow Completed: " + (maxCompleteDate?.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR) ?? "N/A");
					break;
				case ProposalStatus.Archived:
				case ProposalStatus.Deleted:
				case ProposalStatus.NoBid:
					result = proposalStatus.GetDescription();
					break;
			}

			return result;
		}

		/// <summary>
		/// Gets Certification Completed Line for the Revision History
		/// </summary>
		/// <param name="proposalStatus">Proposal Status</param>
		/// <param name="cCoPDRequired">CCOPD Requirewd</param>
		/// <param name="certificationTimelineCompleted">Certification Timeline Completed Date</param>
		/// <returns>Text for the Cert completed line</returns>
		public static string GetCertificationCompletedLineText(ProposalStatus proposalStatus, bool? cCoPDRequired, DateTime? certificationTimelineCompleted)
		{
			string result = string.Empty;

			if (proposalStatus == ProposalStatus.Revised)
			{
				result = "Certification: N/A, Proposal Revised";
			}
			else if (cCoPDRequired == true)
			{
				if (proposalStatus == ProposalStatus.PendingCertification)
				{
					result = "Certification In Progress";
				}
				else if (proposalStatus == ProposalStatus.PendingAward || proposalStatus == ProposalStatus.Lost || proposalStatus == ProposalStatus.Completed)
				{
					result = "Certification Completed: " + certificationTimelineCompleted?.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR) ?? "N/A";
				}
			}

			return result;
		}

		/// <summary>
		/// Initial setting for whether the Certification tab should be displayed.
		/// 
		/// If false, it'll be hidden
		/// If true, it'll get set based on permissions in the controller logic
		/// </summary>
		/// <param name="proposalStatus">Proposal Status</param>
		/// <param name="cCoPDRequired">CCOPD required</param>
		/// <returns>Should Certification Tab be displayed</returns>
		private bool DisplayCertificationTab(ProposalStatus proposalStatus, bool? cCoPDRequired)
		{
			return (proposalStatus == ProposalStatus.PendingCertification || proposalStatus == ProposalStatus.Revised || proposalStatus == ProposalStatus.Completed) && cCoPDRequired == true;
		}

		/// <summary>
		/// Initial setting for whether the Revision tab should be displayed.
		/// 
		/// If false, it'll be hidden
		/// If true, it'll get set based on permissions in the controller logic
		/// </summary>
		/// <param name="isRevision">Is Revision</param>
		/// <param name="proposalStatus">Proposal Status</param>
		/// <returns>Should Display Revision Tab be displayed</returns>
		private bool DisplayRevisionTab(bool isRevision, ProposalStatus proposalStatus)
		{
			return isRevision || proposalStatus == ProposalStatus.Revised;
		}

		/// <summary>
		/// Gets Proposal Data for eEPP application, when the user is searching for a PTM record
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="searchString">Optional search string</param>
		/// <returns>Proposal Data</returns>
		public ICollection<EppProposalData> GetEppProposalData(string ntid, bool isAdmin, string searchString)
		{
			List<EppProposalData> result;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetEppProposalData", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = GetFilteredProposalsForEppRetrieval(dbModel, ntid, isAdmin, searchString)
						.Select(entity => new
						{
							Id = entity.ProposalID,
							TrackingNumber = entity.ProposalTrackingID,
							ProposalTitle = entity.ProposalTitle,
							ProgramAreaId = entity.ProgramAreaID,
							// IES-891: If Revised Anticipated Delivery Date (DB field RevisedSubmittalDate) is available, use that date in place of the Anticipated Delivery Date
							AnticipatedDeliveryDate = entity.RevisedSubmittalDate ?? entity.AnticipatedDeliveryDate,
							LobDescription = entity.LineOfBusinessLU.LineOfBusinessName,
							PaDescription = entity.ProgramAreaLU.ProgramAreaName,
							ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
							Customer = entity.Customer
						}).Take(50).ToList()
						.Select(entity => new EppProposalData()
						{
							ProposalId = entity.Id,
							PTMTrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							LobDescription = entity.LobDescription,
							PaId = entity.ProgramAreaId,
							PaDescription = entity.PaDescription,
							AnticipatedDeliveryDate = entity.AnticipatedDeliveryDate.ToShortDateString(),
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							Customer = entity.Customer
						}).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// This is a helper method used when retrieving data for EPP Application, to limit / filter the proposals
		/// 
		/// The proposals must:
		///     - not be Forecasted
		///     - not have been selected by another eEPP record
		///     
		/// Additionally the user's permissions will limit the allowed proposals.
		/// 
		/// Finally a search can be used to further limit which proposals will be returned
		/// </summary>
		/// <param name="dbModel">DB Model</param>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is User Admin</param>
		/// <param name="searchString">Search String</param>
		/// <returns>Filtered data</returns>
		private static IEnumerable<Proposal> GetFilteredProposalsForEppRetrieval(genTRACEntities dbModel, string ntid, bool isAdmin, string searchString)
		{
			searchString = (searchString ?? string.Empty).Trim().ToLower();

			return dbModel.Proposals.Where(
								x => x.ProposalClassLU.ProposalClass != Constants.PROPOSAL_CLASS_FORECASTED

								// ToDo: Proposal doesn't already have a linked eEPP record (will come later)
								&& true

								&& (isAdmin
										|| x.ProposalUserRoles.Any(role => role.genTRACUser.NTID.ToLower() == ntid && (role.RoleID == (int)PtmRole.ContractsPOC || role.RoleID == (int)PtmRole.BackupContractsPOC)))
								&& (string.IsNullOrEmpty(searchString)
										|| x.ProposalTrackingID.ToLower().Contains(searchString)
										|| x.ProposalTitle.ToLower().Contains(searchString)
										|| x.LineOfBusinessLU.LineOfBusinessName.ToLower().Contains(searchString)
										|| x.ProposalUserRoles.Any(z => z.genTRACUser.DisplayName.ToLower().Contains(searchString) && (z.RoleID == (int)PtmRole.Pricer || z.RoleID == (int)PtmRole.ContractsPOC))
									));
		}

		/// <summary>
		/// Gets Proposal Data for eEPP, by Proposal Tracking Number, when the application needs to check if the previously selected PTM record is out-of-date
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="trackingNumber">Proposal Tracking Number</param>
		/// <returns>Proposal Data</returns>
		public EppProposalData GetEppProposalDataByProposalTrackingNumber(string ntid, bool isAdmin, string trackingNumber)
		{
			EppProposalData result;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetEppProposalDataByProposalTrackingNumber", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals
						.Where(x =>
							(isAdmin
								|| x.ProposalUserRoles.Any(role => role.genTRACUser.NTID.ToLower() == ntid && (role.RoleID == (int)PtmRole.ContractsPOC || role.RoleID == (int)PtmRole.BackupContractsPOC)))
							&& x.ProposalTrackingID == trackingNumber
						)
						.Select(entity => new
						{
							Id = entity.ProposalID,
							TrackingNumber = entity.ProposalTrackingID,
							ProposalTitle = entity.ProposalTitle,
							ProgramAreaId = entity.ProgramAreaID,
							// IES-891: If Revised Anticipated Delivery Date (DB field RevisedSubmittalDate) is available, use that date in place of the Anticipated Delivery Date
							AnticipatedDeliveryDate = entity.RevisedSubmittalDate ?? entity.AnticipatedDeliveryDate,
							LobDescription = entity.LineOfBusinessLU.LineOfBusinessName,
							PaDescription = entity.ProgramAreaLU.ProgramAreaName,
							ContractTypeIds = entity.ContractTypeLUs.Select(x => x.ContractTypeID),
							Customer = entity.Customer
						}).Take(1).ToList()
						.Select(entity => new EppProposalData()
						{
							ProposalId = entity.Id,
							PTMTrackingNumber = entity.TrackingNumber,
							ProposalTitle = entity.ProposalTitle,
							LobDescription = entity.LobDescription,
							PaId = entity.ProgramAreaId,
							PaDescription = entity.PaDescription,
							AnticipatedDeliveryDate = entity.AnticipatedDeliveryDate.ToShortDateString(),
							ContractTypeIds = entity.ContractTypeIds.ToList(),
							Customer = entity.Customer
						}).FirstOrDefault();
				}
			}

			return result;
		}

		/// <summary>
		/// Get options for the Previously Submitted ROM field in the Contracts Tab
		/// </summary>
		/// <param name="selectedValue">Selected option</param>
		/// <returns>Values for dropdown</returns>
		[DbQuery]
		public ICollection<SelectListItem> GetRomProposalOptions(int? selectedValue)
		{
			ICollection<SelectListItem> result;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetRomProposalOptions", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals.Where(x => x.ProposalClassLU.ProposalClass == "ROM")
						.Select(x => new { id = x.ProposalID, text = x.ProposalTrackingID + " " + x.ProposalTitle }).ToList()
						.Select(x => new SelectListItem() { Value = x.id.ToString(), Text = x.text, Selected = x.id == selectedValue }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Gets ROM Proposal's Previously Submitted ROM Value and Previously Submitted ROM Date
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Submitted Date and Submitted Value</returns>
		[DbQuery, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Tuple<DateTime?, decimal?> GetRomDateAndValue(int proposalId)
		{
			Tuple<DateTime?, decimal?> result;

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetRomDateAndValue", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals.Where(x => x.ProposalID == proposalId)
						.Select(x => new { submittedDate = x.ProposalChecklists.FirstOrDefault().ProposalSubmittalDate, submittedValue = x.ProposalChecklists.FirstOrDefault().ISGSTotalPrice }).ToList()
						.Select(x => new Tuple<DateTime?, decimal?>(x.submittedDate, x.submittedValue)).FirstOrDefault();
				}
			}

			return result;
		}

		/// Gets Proposal Data for ACV application, when the user is searching for a PTM record
		/// 
		/// Search criteria:
		///     - proposal is in progress
		///     - user is either system admin, or either a Lead Estimator, or a Backup Lead Est.
		///     - if search string is provided, then a proposal matches if PTM Tracking Number or Proposal Title contain the search string
		///     
		/// The method will return data to top 100 records, as more data being returned to the user is not going to be helpful
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="searchString">Optional search string</param>
		/// <returns>Proposal Data</returns>
		public ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> GetCostVolumeProposalData(string ntid, bool isAdmin, string searchString)
		{
			List<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> result;

			searchString = (searchString ?? string.Empty).Trim().ToLower();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetAcvProposalData", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals.Where(x =>
									x.ProposalStatusID == (int)ProposalStatus.InProgress
									&& (isAdmin || x.ProposalUserRoles.Any(role => role.genTRACUser.NTID.ToLower() == ntid && (role.RoleID == (int)PtmRole.Pricer || role.RoleID == (int)PtmRole.BackupPricer || role.RoleID == (int)PtmRole.CostVolumeLead)))
									&& (string.IsNullOrEmpty(searchString) || x.ProposalTrackingID.ToLower().Contains(searchString) || x.ProposalTitle.ToLower().Contains(searchString)))
						.Select(entity => new { TrackingNumber = entity.ProposalTrackingID, ProposalTitle = entity.ProposalTitle, ProposalId = entity.ProposalID }).Take(100).ToList()
						.Select(entity => (PtmTrackingNumber: entity.TrackingNumber, ProposalTitle: entity.ProposalTitle, ProposalId: entity.ProposalId)).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get the Header data for ACV
		/// </summary>
		/// <param name="proposalId">Proposal ID</param>
		/// <returns>Header data for the proposal ID</returns>
		public AcvHeaderDataDto GetAcvHeaderDataByProposalId(int proposalId)
		{
			AcvHeaderDataDto result = new AcvHeaderDataDto();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetAcvHeaderDataByProposalId", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals.Where(x => x.ProposalID == proposalId).Select(x => new AcvHeaderDataDto()
					{
						PtmTrackingNumber = x.ProposalTrackingID,
						ProposalTitle = x.ProposalTitle,
						RfpNumber = x.RFPNumber,
						ProposalStatus = x.ProposalStatusLU.ProposalStatus,
						// Set date to null if CCoPD required so it will be populated in ACV
						CostVolumeSubmittalDate = x.CCPDRequired.HasValue && x.CCPDRequired.Value ? null
							: x.RevisedSubmittalDate.HasValue ? x.RevisedSubmittalDate : x.AnticipatedDeliveryDate,
						CCLogNumber = x.ProposalContractsDatas1.Any() ? x.ProposalContractsDatas1.FirstOrDefault().ContractsCorrespondLogNumber : string.Empty
					}).FirstOrDefault();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Proposal Roles for the given user that are needed for NLF
		/// </summary>
		/// <param name="ntid">NTID</param>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Collection of Proposals and Roles for the user</returns>
		public ICollection<ProposalRoleDto> GetProposalRolesForNlfByNtid(string ntid, string ptmTrackingNumber = "")
		{
			ICollection<ProposalRoleDto> result = new Collection<ProposalRoleDto>();
			ntid = ntid.ToLower();

			using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetProposalRolesForNlfByNtid", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.Proposals.Where(x => x.ProposalClassLU.ProposalClass != Constants.PROPOSAL_CLASS_FORECASTED
						&& x.ProposalUserRoles.Any(role => role.genTRACUser.NTID.ToLower() == ntid) && (string.IsNullOrEmpty(ptmTrackingNumber) || x.ProposalTrackingID == ptmTrackingNumber))
						.SelectMany(x => x.ProposalUserRoles).Where(role => role.genTRACUser.NTID.ToLower() == ntid
							&& (role.RoleID == (int)PtmRole.Pricer || role.RoleID == (int)PtmRole.BackupPricer
								|| role.RoleID == (int)PtmRole.CostVolumeLead || role.RoleID == (int)PtmRole.SupplyChainPOCMatl
								|| role.RoleID == (int)PtmRole.BackupMaterialLead || role.RoleID == (int)PtmRole.SupplyChainPOCSubs
								|| role.RoleID == (int)PtmRole.BackupSubcontractsLead))
						.Select(x => new ProposalRoleDto()
						{
							TrackingNumber = x.Proposal.ProposalTrackingID,
							Role = (PtmRole)x.RoleID
						}).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Lead Estimator and Backup Estimator names for a tracking number
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM tracking number</param>
		public ICollection<ProposalRoleDto> GetEstimatorNames(string ptmTrackingNumber)
		{
			ICollection<ProposalRoleDto> names;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (genTRACEntities gte = new genTRACEntities())
				{
					names = (from p in gte.Proposals
							 join pur in gte.ProposalUserRoles on p.ProposalID equals pur.ProposalID
							 join gtu in gte.genTRACUsers on pur.UserID equals gtu.UserID
							 where p.ProposalTrackingID == ptmTrackingNumber
							 && (pur.RoleID == (int)PtmRole.Pricer || pur.RoleID == (int)PtmRole.BackupPricer)
							 select new ProposalRoleDto
							 {
								 TrackingNumber = p.ProposalTrackingID,
								 Role = (PtmRole)pur.RoleID,
								 UserName = gtu.DisplayName,
								 NTID = gtu.NTID
							 }).ToList();
				}
			}

			return names;
		}
	}
}