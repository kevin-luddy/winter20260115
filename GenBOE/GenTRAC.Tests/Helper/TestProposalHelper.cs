// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Helper
{
    using System;
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects.FullObject;
    using IES.Common;

    /// <summary>
    /// Helper to create various proposal objects for mocking
    /// </summary>
    public static class TestProposalHelper
    {
        /// <summary>
        /// Aggregate method to create a full proposal
        /// </summary>
        /// <param name="proposalId">Optional proposal Id</param>
        /// <param name="proposalStatus">Status of the proposal</param>
        /// <returns>FullProposal object</returns>
        public static FullProposal GetFullProposalForMocks(int proposalId = 1, ProposalStatus proposalStatus = ProposalStatus.InProgress)
        {
            FullProposal fullProposal = new FullProposal(GetProposalDtoForMocks(proposalId));
            fullProposal.Permissions.AddRange(GetPermissionsForMocks());
            fullProposal.ProposalStatus = proposalStatus;

            return fullProposal;
        }

        /// <summary>
        /// Get user DTO for mocking
        /// </summary>
        /// <param name="userId">Optional user Id</param>
        /// <returns>User object</returns>
        public static UserDTO GetLeadEstimatorUserForMocks(int userId = 1)
        {
            return new UserDTO
            {
                Id = userId,
                DisplayName = "Mock User (Estimator)",
                FirstName = "Mock",
                LastName = "User",
                Ntid = "n0001e",
                EmailAddress = "mock.user@lmco.com",
                IsUsPerson = true,
                UserType = UserType.User
            };
        }

        /// <summary>
        /// Get user DTO for Contracts User for mocking
        /// </summary>
        /// <param name="userId">Optional user Id</param>
        /// <returns>DTO representing the contracts poc</returns>
        public static UserDTO GetLeadContractsUserForMocks(int userId = 6)
        {
            return new UserDTO
            {
                Id = userId,
                DisplayName = "Mock User (Contracts PoC)",
                FirstName = "Mock",
                LastName = "User",
                Ntid = "n0002e",
                EmailAddress = "mock.user@lmco.com",
                IsUsPerson = true,
                UserType = UserType.User
            };
        }

        /// <summary>
        /// Get proposal DTO for mocking
        /// </summary>
        /// <param name="proposalId">Optional proposal Id</param>
        /// <returns>FullProposal object</returns>
        public static ProposalDto GetProposalDtoForMocks(int proposalId = 1)
        {
            return new ProposalDto()
            {
                Id = proposalId,
                LeadEstimatorSignedDate = DateTime.Today,
                LeadEstimatorSignatureComment = "LE Comment",
                CoverSheetApproverSignedDate = DateTime.Today.AddDays(1),
                CoverSheetApproverSignatureComment = "CSA Comment",
                PricingVerifierSignedDate = DateTime.Today.AddDays(2),
                PricingVerifierSignatureComment = "PV Comment",
                IndependentReviewerSignedDate = DateTime.Today.AddDays(3),
                IndependentReviewerSignatureComment = "IR Comment",
                LOBEstimatingLeadSignedDate = null,
                LOBEstimatingLeadSignatureComment = string.Empty
            };
        }

        /// <summary>
        /// Get proposal permission set for mocking
        /// </summary>
        /// <returns>List of ProposalPermissionDTOs</returns>
        public static List<ProposalPermissionDto> GetPermissionsForMocks()
        {
            ProposalPermissionDto leadEstimator = new ProposalPermissionDto()
            {
                UserId = 1,
                Role = PtmRole.Pricer
            };

            ProposalPermissionDto coverSheetApprover = new ProposalPermissionDto()
            {
                UserId = 2,
                Role = PtmRole.CoverSheetApprover
            };

            ProposalPermissionDto pricingVerification = new ProposalPermissionDto()
            {
                UserId = 3,
                Role = PtmRole.PricingVerification
            };

            ProposalPermissionDto independentReviewer = new ProposalPermissionDto()
            {
                UserId = 4,
                Role = PtmRole.PeerReviewer
            };

            ProposalPermissionDto lobEstimatingLead = new ProposalPermissionDto()
            {
                UserId = 5,
                Role = PtmRole.LOBEstLead
            };

            ProposalPermissionDto contractsPOC = new ProposalPermissionDto
            {
                UserId = 6,
                Role = PtmRole.ContractsPOC
            };

            return new List<ProposalPermissionDto>() { leadEstimator, coverSheetApprover, pricingVerification, independentReviewer, lobEstimatingLead, contractsPOC };
        }
    }
}
