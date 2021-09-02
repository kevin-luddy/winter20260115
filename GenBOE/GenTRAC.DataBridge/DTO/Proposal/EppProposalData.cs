// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Proposal Data for eEPP
    /// </summary>
    [Serializable]
    public class EppProposalData
    {
        /// <summary>
        /// Proposal Id, used for unit tests, not actually sent down w/ JSON
        /// </summary>
        [JsonIgnore]
        public int ProposalId { get; set; }

        /// <summary>
        /// PTM Tracking Number
        /// </summary>
        public string PTMTrackingNumber { get; set; }

        /// <summary>
        /// Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Line of business Description
        /// </summary>
        public string LobDescription { get; set; }

        /// <summary>
        /// Program area id
        /// </summary>
        public int PaId { get; set; }

        /// <summary>
        /// Program area Description
        /// </summary>
        public string PaDescription { get; set; }

        /// <summary>
        /// Anticipated Delivery date
        /// </summary>
        public string AnticipatedDeliveryDate { get; set; }

        /// <summary>
        /// Customer
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// Contract Type Ids
        /// </summary>
        public ICollection<int> ContractTypeIds { get; set; }
    }
}
