// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    /// <summary>
    /// Class used to send proposal data for CV creation
    /// </summary>
    public class AcvProposalData
    {
        /// <summary>
        /// PTM Tracking Number
        /// </summary>
        public string PtmTrackingNumber { get; set; }

        /// <summary>
        /// Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Id of Proposal
        /// </summary>
        public int ProposalId { get; set; }
    }
}