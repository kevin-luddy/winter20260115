// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contracts DTO
    /// </summary>
    [Serializable]
    public class ContractsDto : IES.Common.UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsDto"/> class.
        /// </summary>
        public ContractsDto()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the Proposal Id that this Contract is linked to.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Previously Submitted ROM (PTM record)
        /// </summary>
        public int? PreviouslySubmittedROM { get; set; }

        /// <summary>
        /// Customer Submittal Date string
        /// </summary>
        public DateTime? CustomerSubmittalDate { get; set; }

        /// <summary>
        /// Cage #
        /// </summary>
        public string CageCode { get; set; }

        /// <summary>
        /// Contracts Correspondence Log Number
        /// </summary>
        public string ContractsCorrespondenceLogNumber { get; set; }

        /// <summary>
        /// Final Negotiated Value string
        /// </summary>
        public long? FinalNegotiatedValue { get; set; }

        /// <summary>
        /// Date Confirmation of Negotiations Submitted string
        /// </summary>
        public DateTime? NegotiationsSubmitted { get; set; }

        /// <summary>
        /// EPP Delegation Authority ID
        /// </summary>
        public int? EppDelegationAuthority { get; set; }

        /// <summary>
        /// Program EPP Date
        /// </summary>
        public DateTime? ProgramEppDate { get; set; }

        /// <summary>
        /// LOB EPP Date
        /// </summary>
        public DateTime? LobEppDate { get; set; }

        /// <summary>
        /// Pre Space EPP Date
        /// </summary>
        public DateTime? PreSpaceEppDate { get; set; }

        /// <summary>
        /// Space EPP Date
        /// </summary>
        public DateTime? SpaceEppDate { get; set; }

        /// <summary>
        /// Pre Corporate EPP Date
        /// </summary>
        public DateTime? PreCorporateEppDate { get; set; }

        /// <summary>
        /// Corporate Epp Date
        /// </summary>
        public DateTime? CorporateEppDate { get; set; }

        /// <summary>
        /// EPP ROS Delegation Notes
        /// </summary>
        public string EppRosDelegationNotes { get; set; }

        /// <summary>
        /// LM Won contract
        /// </summary>
        public bool? LmWon { get; set; }

        /// <summary>
        /// Mod Completed Date
        /// </summary>
        public DateTime? ModCompletedDate { get; set; }
    }
}
