// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// RDSB Data driven CPS Report Request w/ Rate Descriptions
    /// </summary>
    public class RDSBRateDescriptionsRequest
    {
        /// <summary>
        /// Rate Descriptions.
        /// </summary>
        public ICollection<string> RateDescriptions { get; set; }

        /// <summary>
        /// PTM Proposal Id.
        /// </summary>
        public int PtmProposalId { get; set; }
    }
}