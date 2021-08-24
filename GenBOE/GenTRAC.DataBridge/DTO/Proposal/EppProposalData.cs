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
    /// Proposal Data for eEPP
    /// </summary>
    [Serializable]
    public class EppProposalData
    {
        /// <summary>
        /// Proposal Id
        /// </summary>
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
        /// Contract Types
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ICollection<KeyValuePair<int, string>> ContractTypes { get; set; }
    }
}
