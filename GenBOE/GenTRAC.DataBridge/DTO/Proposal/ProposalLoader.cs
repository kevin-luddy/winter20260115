// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Proposal dto data loader
    /// </summary>
    public class ProposalLoader : DataLoader<ProposalDto>, IProposalLoader
    {
        /// <summary>
        /// Gets or sets the Permission Loader.
        /// </summary>
        private IProposalPermissionLoader PermissionLoader { get; set; }

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
        /// Initializes a new instance of the <see cref="ProposalLoader"/> class.
        /// </summary>
        /// <param name="permissionLoader">The permission loader.</param>
        public ProposalLoader(IProposalPermissionLoader permissionLoader) : this()
        {
            this.PermissionLoader = permissionLoader;
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
                    var resultLinq = from x in dbModel.Proposals
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
                            InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
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
                            (int)dtoToUpsert.ProposalType,
                            false,
                            dtoToUpsert.ProgramName,
                            dtoToUpsert.Customer,
                            (int)dtoToUpsert.CustomerType,
                            (int)dtoToUpsert.ISGSRole,
                            dtoToUpsert.Request,
                            dtoToUpsert.RFPNumber,
                            (int)dtoToUpsert.LineOfBusinessID,
                            (int)dtoToUpsert.ProgramAreaId,
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
                            (int)dtoToUpsert.ContractTypeGroup,
                            dtoToUpsert.IsScheduleProposal,
                            (int)dtoToUpsert.ProposalLocation,
                            dtoToUpsert.ProposalLocationName = (dtoToUpsert.ProposalLocation != ProposalLocation.Other) ? string.Empty : dtoToUpsert.ProposalLocationName,
                            dtoToUpsert.BoeToolName = (dtoToUpsert.BoeTool != BOETool.Other) ? string.Empty : dtoToUpsert.BoeToolName,
                            dtoToUpsert.PricingToolName = (dtoToUpsert.PricingTool != PricingTool.Other) ? string.Empty : dtoToUpsert.PricingToolName,
                            (int)((ProposalChecklistType.InternationalCommercial.IsActive() && (dtoToUpsert.CustomerType == CustomerType.InternationalCommercial || dtoToUpsert.CustomerType == CustomerType.StateAndLocal || dtoToUpsert.CustomerType == CustomerType.Commercial)) ? ProposalChecklistType.InternationalCommercial : ProposalChecklistType.Default),
                            dtoToUpsert.ChangeChecklist,
                            (int)dtoToUpsert.ProgramProposalStatus,
                            (int)dtoToUpsert.ProposalClass,
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
                            dtoToUpsert.ProposalSetupComments).FirstOrDefault();
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
                    var completed = (from p in dbModel.Proposals
                                     where p.ProposalID == inProposalId
                                     select p.ProposalStatusID).First();

                    // as long as the proposal is not in progress, grab the approval completed date
                    if (completed == (int)ProposalStatus.Completed || completed == (int)ProposalStatus.Submitted || completed == (int)ProposalStatus.Revised)
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
                                join c in dbModel.ProposalChecklistCompletes
                                on p.ProposalID equals c.ProposalID
                                where (p.ProposalStatusID == (int)ProposalStatus.Completed || p.ProposalStatusID == (int)ProposalStatus.Submitted)
                                && c.SubmitDate >= cutoffDate && c.ChecklistTypeID == (int)ProposalChecklistType.Default
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
                                    IsForecastProposal = p.ProposalClassLU.ProposalClass == Constants.PROPOSAL_CLASS_FORECASTED
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
                var proposals = dbModel.Proposals;

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
                        InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
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
                        InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
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
                            InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
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
                            InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
                        }).ToList();
                }
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
                    toReturn = dbModel.Proposals.Where(p => p.ProposalStatusID == (int)ProposalStatus.Submitted &&
                    (p.CertificationLastEmailed == null || p.CertificationLastEmailed < thirtyDaysAgo) &&
                    (!p.ProposalChecklists.Any() || (p.ProposalChecklists.Select(c => c.ProposalSubmittalDate).Max() < sixtyDaysAgo)))
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
                            InformationComments = entity.InformationComments
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
                            WorkflowStatus = (WorkflowStatus)entity.WorkflowStatus,
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
                            ManageProposalInfoComments = entity.InformationComments
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
                var forecastedTracking = from x in dbModel.Proposals
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

            switch(proposalStatus)
            {
                case ProposalStatus.InProgress:
                    result = "Due: " + (revisedDeliveryDate.HasValue ? revisedDeliveryDate.Value.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR) 
                        : anticipatedDeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)); 
                    break;
                case ProposalStatus.Completed:
                case ProposalStatus.Submitted:
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
                if (proposalStatus == ProposalStatus.Submitted)
                {
                    result = "Certification In Progress";
                }
                else if (proposalStatus == ProposalStatus.Completed)
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
            return (proposalStatus == ProposalStatus.Submitted || proposalStatus == ProposalStatus.Revised || proposalStatus == ProposalStatus.Completed) && cCoPDRequired == true;
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
        /// Gets Proposal Data for eEPP
        /// </summary>
        /// <param name="ntid">User's NTID</param>
        /// <param name="isAdmin">Is the user System Admin</param>
        /// <param name="searchString">Optional search string</param>
        /// <returns>Proposal Data</returns>
        public ICollection<EppProposalData> GetEppProposalData(string ntid, bool isAdmin, string searchString)
        {
            searchString = (searchString ?? string.Empty).ToLower();

            List<EppProposalData> result;
            using (StopwatchTimer sw = new StopwatchTimer("ProposalLoader.GetByIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    result = dbModel.Proposals
                        .Where(x => 
                            x.ProposalClassLU.ProposalClass != Constants.PROPOSAL_CLASS_FORECASTED
                            && x.ProposalStatusID == (int)ProposalStatus.InProgress

                            // ToDo: Proposal doesn't already have a linked eEPP record (will come later)
                            && true 
                            
                            && (isAdmin
                                // ToDo: once we have Backup Contracts Lead, add the role into the 2nd role comparison
                                || x.ProposalUserRoles.Any(role => role.genTRACUser.NTID.ToLower() == ntid && (role.RoleID == (int)PtmRole.ContractsPOC || role.RoleID == (int)PtmRole.ContractsPOC)))

                            && (string.IsNullOrEmpty(searchString) 
                                    || x.ProposalTrackingID.ToLower().Contains(searchString)
                                    || x.ProposalTitle.ToLower().Contains(searchString)
                                    || x.LineOfBusinessLU.LineOfBusinessName.ToLower().Contains(searchString)
                                    || x.ProposalUserRoles.Any(z => z.genTRACUser.DisplayName.ToLower().Contains(searchString) && (z.RoleID == (int)PtmRole.Pricer || z.RoleID == (int)PtmRole.ContractsPOC))
                                )
                        )
                        .Select(entity => new
                        {
                            Id = entity.ProposalID,
                            TrackingNumber = entity.ProposalTrackingID,
                            ProposalTitle = entity.ProposalTitle,
                            LineOfBusinessID = entity.LineOfBusinessID,
                            ProgramAreaId = entity.ProgramAreaID,
                            AnticipatedDeliveryDate = entity.AnticipatedDeliveryDate,
                            LobDescription = entity.LineOfBusinessLU.LineOfBusinessName,
                            PaDescription = entity.ProgramAreaLU.ProgramAreaName,
                            ContractTypeLUs = entity.ContractTypeLUs
                        }).Take(50).ToList()
                        .Select(entity => new EppProposalData()
                        {
                            ProposalId = entity.Id,
                            PTMTrackingNumber = entity.TrackingNumber,
                            ProposalTitle = entity.ProposalTitle,
                            LobId = entity.LineOfBusinessID,
                            LobDescription = entity.LobDescription,
                            PaId = entity.ProgramAreaId,
                            PaDescription = entity.PaDescription,
                            AnticipatedDeliveryDate = entity.AnticipatedDeliveryDate,
                            ContractTypes = entity.ContractTypeLUs.Select(x => new KeyValuePair<int, string>(x.ContractTypeID, x.ContractType)).ToList()
                        }).ToList();
                }
            }

            return result;
        }
    }
}
