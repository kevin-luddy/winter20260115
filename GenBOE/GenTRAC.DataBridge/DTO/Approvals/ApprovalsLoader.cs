// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Approvals Loader
    /// </summary>
    public class ApprovalsLoader : IApprovalsLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        private IES.Common.Logger Log { get; set; }

        /// <summary>
        /// Default Ctor
        /// </summary>
        public ApprovalsLoader()
        {
            this.Log = new IES.Common.Logger("ApprovalsLoader");
        }

        /// <summary>
        /// Gets all approvals for the user
        /// </summary>
        /// <param name="userId">The users genTrac user ID.</param>
        /// <returns>A list of proposals for the given user.</returns>
        public ICollection<ApprovalsDto> GetApprovalsForUser(int userId)
        {
            ICollection<ApprovalsDto> toReturn = new List<ApprovalsDto>();

            List<int> approvalRoles = new List<int>() { (int)PtmRole.CoverSheetApprover, (int)PtmRole.PeerReviewer, (int)PtmRole.Pricer, (int)PtmRole.LOBEstLead, (int)PtmRole.PricingVerification };

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsLoader.GetApprovalsForUser", this.Log))
            {
                using (genTRACEntities gbe = new genTRACEntities())
                {
                    toReturn = gbe.ProposalUserRoles.Where(x => x.UserID == userId && approvalRoles.Contains(x.RoleID)
                        && x.Proposal.LeadEstimatorSignedDT.HasValue)
                        .Select(x => new ApprovalsDto()
                        {
                            SubmittedForApprovalDate = x.Proposal.LeadEstimatorSignedDT.Value,
                            ProposalId = x.ProposalID,
                            ProposalTitle = x.Proposal.ProposalTitle,
                            ProposalStatus = (ProposalStatus)x.Proposal.ProposalStatusID,
                            TrackingNumber = x.Proposal.ProposalTrackingID,
                            CaptureManager = x.Proposal.ProposalUserRoles.FirstOrDefault(z => z.RoleID == (int)PtmRole.CaptureManager).genTRACUser.DisplayName,

                            UserRole = (PtmRole)x.RoleID,
                            ApprovedDate =
                                x.RoleID == (int)PtmRole.CoverSheetApprover ? x.Proposal.CoverSheetApproverSignedDT :
                                (x.RoleID == (int)PtmRole.PeerReviewer ? x.Proposal.IndependentReviewerSignedDT :
                                 (x.RoleID == (int)PtmRole.Pricer ? x.Proposal.LeadEstimatorSignedDT :
                                  (x.RoleID == (int)PtmRole.LOBEstLead ? x.Proposal.LOBEstimatingLeadSignedDT :
                                   (x.RoleID == (int)PtmRole.PricingVerification ? x.Proposal.PricingVerifierSignedDT : null
                            ))))
                        }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets # of pending approvals for the user
        /// </summary>
        /// <param name="userId">The users genTrac user ID.</param>
        /// <returns>The number of pending proposals.</returns>
        public int GetPendingApprovalsCountForUser(int userId)
        {
            int result = 0;
            
            List<int> approvalRoles = new List<int>() { (int)PtmRole.CoverSheetApprover, (int)PtmRole.PeerReviewer, (int)PtmRole.Pricer, (int)PtmRole.LOBEstLead, (int)PtmRole.PricingVerification };

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsLoader.GetPendingApprovalsCountForUser", this.Log))
            {
                using (genTRACEntities gbe = new genTRACEntities())
                {
                    // Get the proposer user roles for the user
                    result = gbe.ProposalUserRoles.Where(x => x.UserID == userId && approvalRoles.Contains(x.RoleID)
                        // Where the Lead Estimator has signed (i.e. approval workflow has started)
                        && x.Proposal.LeadEstimatorSignedDT.HasValue &&
                        // if the role is Cover Sheet Approver, only count if it hasn't been approved for that role yet
                        !(x.RoleID == (int)PtmRole.CoverSheetApprover ? x.Proposal.CoverSheetApproverSignedDT.HasValue :
                            // if the role is Independent Reviewer, only count if it hasn't been approved for that role yet
                            (x.RoleID == (int)PtmRole.PeerReviewer ? x.Proposal.IndependentReviewerSignedDT.HasValue :
                                // if the role is Lead Estimator, only count if it hasn't been approved for that role yet
                                (x.RoleID == (int)PtmRole.Pricer ? x.Proposal.LeadEstimatorSignedDT.HasValue :
                                    // if the role is LOB Est Lead, only count if it hasn't been approved for that role yet
                                    (x.RoleID == (int)PtmRole.LOBEstLead ? x.Proposal.LOBEstimatingLeadSignedDT.HasValue :
                                        // if the role is Pricing Verification, only count if it hasn't been approved for that role yet
                                        (x.RoleID == (int)PtmRole.PricingVerification ? x.Proposal.PricingVerifierSignedDT.HasValue : true)))))
                        // if the role is LOB Est Lead, only count if Cover Sheet Approver has approved, Pricing Verification has approved,
                        && (x.RoleID == (int)PtmRole.LOBEstLead ? x.Proposal.CoverSheetApproverSignedDT.HasValue && x.Proposal.PricingVerifierSignedDT.HasValue 
                            // and, if Independent Reviewer is required (Lead Estimator and Cover Sheet Approver are the same),
                            && ((x.Proposal.ProposalUserRoles.FirstOrDefault(y => y.RoleID == (int)PtmRole.Pricer).UserID == x.Proposal.ProposalUserRoles.FirstOrDefault(y => y.RoleID == (int)PtmRole.CoverSheetApprover).UserID) ?
                                // independent reviewer has approved
                                x.Proposal.IndependentReviewerSignedDT.HasValue : true) : true))
                        .Count();
                }
            }

            return result;
        }
    }
}
