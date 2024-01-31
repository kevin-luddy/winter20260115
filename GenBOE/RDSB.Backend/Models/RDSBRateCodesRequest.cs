// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Models
{
	using System.Collections.Generic;

	/// <summary>
	/// Trace Table Settings Data
	/// </summary>
	public class RDSBRateCodesRequest
	{
        /// <summary>
        /// Rate Codes.
        /// </summary>
        public ICollection<string> RateCodes { get; set; }

        /// <summary>
        /// PTM Proposal Id.
        /// </summary>
        public int PtmProposalId { get; set; }
    }
}