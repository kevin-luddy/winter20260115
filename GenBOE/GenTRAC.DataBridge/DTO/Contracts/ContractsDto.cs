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
        /// Customer Submittal Date String
        /// </summary>
        public DateTime? CustomerSubmittalDate { get; set; }

        /// <summary>
        /// Contracts Correspondence Log Number
        /// </summary>
        public string ContractsCorrespondenceLogNumber { get; set; }

        /// <summary>
        /// Final Negotiated Value String
        /// </summary>
        public long? FinalNegotiatedValue { get; set; }

        /// <summary>
        /// Date Confirmation of Negotiations Submitted String
        /// </summary>
        public DateTime? NegotiationsSubmitted { get; set; }

        /// <summary>
        /// Contract Offers
        /// </summary>
        public ICollection<ContractsOffersDto> ContractOffers { get; set; } = new List<ContractsOffersDto>();
    }
}
